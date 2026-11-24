#!/bin/sh
# Brings the demo up on the server, run by scripts/deploy-demo.ps1 as: sudo sh deploy/bootstrap.sh
#   1. database passwords on the first run (kept in deploy/.env, mode 600);
#   2. comp-ledger, then the demo tenant with the demo employees, once (its id goes to deploy/.env);
#   3. paragraf, which loads the code into pgvector on its first start.
set -eu
cd "$(dirname "$0")"
touch .env
chmod 600 .env

random() { head -c 16 /dev/urandom | od -An -tx1 | tr -d ' \n'; }
for name in PARAGRAF_DB_PASSWORD COMP_LEDGER_DB_PASSWORD COMP_LEDGER_APP_PASSWORD; do
  grep -q "^$name=." .env || echo "$name=$(random)" >> .env
done
for name in ANTHROPIC_API_KEY OPENAI_API_KEY; do
  grep -q "^$name=." .env || { echo "No $name in deploy/.env: run scripts/deploy-demo.ps1 -Secrets"; exit 1; }
done

project=${COMPOSE_PROJECT_NAME:-paragraf}
dc="docker compose -p $project -f docker-compose.yml"
curl="docker run --rm --network ${project}_default curlimages/curl:8.10.1 -sf"

$dc up -d --build comp-ledger-db comp-ledger
echo "Waiting for comp-ledger…"
i=0
until $curl http://comp-ledger:8080/health >/dev/null 2>&1; do
  i=$((i + 1))
  [ $i -lt 60 ] || { echo "comp-ledger did not become healthy"; $dc logs --tail=40 comp-ledger; exit 1; }
  sleep 2
done

if ! grep -q '^COMP_LEDGER_TENANT_ID=.' .env; then
  tenant=$($curl -X POST -H 'Content-Type: application/json' -d '{"name":"paragraf demo"}' http://comp-ledger:8080/api/v1/tenants \
    | sed -n 's/.*"id":"\([^"]*\)".*/\1/p')
  [ -n "$tenant" ] || { echo "Could not create the demo tenant in comp-ledger"; exit 1; }
  # The file goes over stdin: no bind mount, so the same line works wherever docker runs.
  docker run --rm -i --network ${project}_default curlimages/curl:8.10.1 -sf \
    -X POST -H "X-Tenant-Id: $tenant" -H 'Content-Type: text/csv; charset=utf-8' --data-binary @- \
    http://comp-ledger:8080/api/v1/ingestions < ../evals/payroll/employees.csv >/dev/null
  echo "COMP_LEDGER_TENANT_ID=$tenant" >> .env
  echo "Demo tenant created in comp-ledger with the demo employees."
fi

$dc up -d --build
docker image prune -f >/dev/null
echo "Up."
