#!/bin/sh
# comp-ledger connects as a login in the comp_app role (no superuser, no BYPASSRLS); its migrations require the role.
set -eu
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<SQL
CREATE ROLE comp_app NOLOGIN;
CREATE ROLE comp_app_login LOGIN PASSWORD '$COMP_APP_PASSWORD' IN ROLE comp_app;
SQL
