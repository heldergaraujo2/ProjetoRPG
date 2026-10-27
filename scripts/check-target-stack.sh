#!/usr/bin/env sh
set -eu

npm run check
npm run test:target:shared
npm run test:target:server
npm run test:target:client
