# Recuperação de documentos substituídos

Em 2026-10-27, a auditoria encontrou versões anteriores como blobs Git sem referência (`git fsck --no-reflogs --unreachable`). O conteúdo foi copiado para este diretório, sem alterar HEAD nem a branch. Os IDs de objeto documentam exatamente a origem:

- `README-before-official-import.md` — blob `46b7204db5157b4ddc297736d288233865eb0af3`.
- `02_ARQUITETURA-before-reconciliation.md` — blob `16fe80e5dcc3c20a48c69127b4db4d87b4bc7035`.
- `ADR-001-before-version-audit.md` — blob `fefd211e3e1f5819ee34114fe8aa4014558bf574`.
- `client-README-before-scaffold.md` — blob `6c855f6546c36473b18d1cc8c920ac4d964d5a81`.
- `server-README-before-scaffold.md` — blob `5ddf11ad7d78d3180704775308f6437da9bc2a85`.
- `infra-README-before-scaffold.md` — blob `dcba19612c2fd432d36eec0ca5c4edb47207fbf5`.
- `package-before-target-scaffold.json` e `gitignore-before-scaffold.txt` — reconstruídos dos conteúdos legíveis no histórico desta sessão (não há garantia de correspondência com um blob Git).

Há outros blobs próximos para README/arquitetura que diferem apenas na presença de detalhes sobre `npm run doctor`; não é possível provar qual snapshot era a última versão não commitada que estava no working tree antes da substituição. Portanto, este arquivo é uma recuperação útil e substancial, **não uma alegação de restauração byte a byte do último estado local**.

Estes documentos são arquivo histórico, não fontes operacionais. A fonte vigente é `README.md`, `docs/02_ARQUITETURA.md`, `docs/adr/ADR-001-stack.md` e `CHECKPOINT.md`. Não os copie de volta por cima dos documentos ativos.
