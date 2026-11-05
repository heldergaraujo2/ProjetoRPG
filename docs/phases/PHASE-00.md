# FASE 0 — PREPARAÇÃO, AUDITORIA E DEFINIÇÃO ARQUITETURAL

**Estado inicial:** NÃO INICIADA  
**Objetivo:** transformar a proposta em uma base técnica confirmada, reproduzível e segura, sem começar gameplay prematuramente.

## 1. Leitura obrigatória
Ler README.md, rpg-pack/CLAUDE.md, rpg-pack/CHECKPOINT.md, rpg-pack/00_LEIA-ME_PRIMEIRO.md, 01_VISAO_E_ESCOPO.md, 02_ARQUITETURA.md, 08_FASES.md, 09_TESTES_QUALIDADE.md, 11_SEGURANCA.md, 12_LEGAL_ORIGINALIDADE.md e, se existirem, ASSETS_LICENCAS.md e THIRD_PARTY.md. O checkpoint em rpg-pack é a fonte oficial; não criar outro.

## 2. Preservação e segurança
- Confirmar repositório, branch, diretório e git status antes de editar.
- Preservar alterações locais, arquivos não rastreados, assets e dados.
- Não usar reset destrutivo, clean, exclusões em massa ou sobrescritas sem autorização.
- Atuar somente no ProjetoRPG; não alterar outros repositórios.
- Não versionar credenciais, tokens, senhas ou arquivos de ambiente com segredos.
- Não publicar portas nem configurar produção.
- Não copiar código, arte, mapas, nomes, textos ou dados proprietários de outros jogos.

## 3. Auditoria do repositório
### 3.1 Inventário
- Mapear a árvore real, projetos, scripts, assets, dados, testes, CI, documentação e arquivos gerados.
- Identificar linguagens, engines, SDKs, runtimes, gerenciadores de pacotes e ferramentas.
- Identificar dependências, arquivos grandes, binários e licenças.
- Classificar componentes como funcional, protótipo, placeholder, exemplo, legado ou gerado.
- Identificar duplicações, instruções conflitantes e documentação desatualizada.
- Não presumir que caminhos do documento arquitetural já existem.

### 3.2 Git
- Registrar branch, commit, estado limpo/sujo e alterações locais.
- Verificar .gitignore, arquivos rastreados e tamanho dos assets.
- Não reescrever histórico nem descartar trabalho do usuário.

### 3.3 Executabilidade
- Verificar projetos Godot, soluções/projetos C#, scripts, testes e CI existentes.
- Confirmar versões e comandos reais.
- Executar primeiro inspeções; depois builds e testes existentes que sejam seguros.
- Registrar saída literal, código de saída, duração e ambiente.

## 4. Validar a stack e produzir ADR-001
A stack atual é proposta, não decisão confirmada. Avaliar Godot 4 C#; servidor dedicado .NET 8/C#; biblioteca Shared; PostgreSQL e eventual SQLite local; TCP versus UDP confiável/ENet/LiteNetLib ou opção inicial mais simples; JSON + JSON Schema versus alternativas; xUnit e testes headless. Considerar compatibilidade, latência, segurança, licenças, maturidade, manutenção, custo operacional e plataformas-alvo.

Criar docs/adr/ADR-001-stack.md com contexto, opções, prós/contras, riscos, decisão, consequências e status PROPOSTO/ACEITO/REJEITADO. Só marcar ACEITO quando a decisão estiver sustentada por evidências e compatibilidade verificadas. Evitar benchmarks desnecessários.

## 5. Arquitetura e estrutura
Comparar rpg-pack/02_ARQUITETURA.md com a árvore existente. Definir a estrutura mínima coerente para Shared, Server, Client, Tools, content, tests e docs. Não criar dezenas de diretórios vazios nem mover código apenas para corresponder ao desenho. Documentar o mapeamento entre estrutura atual e alvo.

## 6. Ambiente reproduzível
- Identificar versões exatas de Godot, .NET SDK, banco, Git e ferramentas.
- Documentar preparação de máquina limpa.
- Definir configuração local segura e exemplos sem segredos.
- Criar comandos únicos de build e teste, preferindo scripts compatíveis com Windows PowerShell e CI.
- Preparar build do servidor e import/build headless do cliente conforme stack validada.
- Criar teste mínimo real para cada componente executável.
- Se não houver projeto executável, criar somente esqueleto mínimo aprovado pelo ADR; nunca simular testes.
- Configurar CI simples para executar os mesmos comandos locais, se suportado.
- Registrar licenças de bibliotecas e assets em THIRD_PARTY.md e ASSETS_LICENCAS.md.

## 7. Conteúdo e schemas
- Confirmar/criar estrutura content conforme arquitetura.
- Criar schemas estruturais mínimos para os tipos previstos, sem inventar balanceamento final.
- Validar exemplos existentes do pacote.
- Definir IDs string estáveis e versão de conteúdo.
- Conteúdo inválido deve ser detectável por ferramenta/teste; bloqueio de startup do servidor será concluído na Fase 1.

## 8. Observabilidade mínima
Definir formato de logs, níveis e coleta de versões, duração de build/teste e códigos de saída. Não registrar segredos. Não implementar métricas completas do servidor se ainda não houver servidor; isso pertence à Fase 1.

## 9. Testes obrigatórios
1. Checkout limpo ou ambiente equivalente documentado.
2. Build de Shared/Server, quando aplicável.
3. Build/import headless do cliente, quando aplicável.
4. Teste automatizado mínimo de Shared/Server, quando aplicável.
5. Teste mínimo do cliente, quando suportado.
6. Validação de schemas e exemplos de conteúdo.
7. Comandos executados conforme documentação.
8. CI executando os mesmos comandos, se configurada.
9. Repetição dos testes após correções.
10. Registro de testes não executáveis e motivos.

## 10. Critérios de aceite
- Inventário técnico com evidências.
- Stack validada e ADR-001 registrado.
- Estrutura atual comparada com arquitetura proposta.
- Build do cliente e servidor executável do zero, ou bloqueios concretos documentados.
- Pelo menos um teste real passando para cada componente executável.
- Comandos reproduzíveis de build, teste, execução e validação de conteúdo.
- CI simples funcionando, quando aplicável.
- Schemas/conteúdo inicial validados.
- Licenças e dependências registradas.
- rpg-pack/CHECKPOINT.md atualizado sem apagar histórico.
- docs/reports/fase00.md com comandos, saídas, versões, evidências, falhas e limitações.

## 11. Fora do escopo
Login completo, rede de gameplay, sincronização multiplayer, mundo, movimento, combate, monstros, IA, inventário, drops, economia, progressão, eventos, publicação de servidor e migração de dados de produção.

## 12. Ordem de execução
1. Auditoria somente leitura.
2. Diagnóstico de ambiente.
3. Comparação da arquitetura com código existente.
4. ADR-001.
5. Estrutura mínima e scripts reproduzíveis.
6. Testes mínimos reais de cliente/servidor.
7. Schemas e validação de conteúdo.
8. CI e documentação operacional.
9. Regressão completa.
10. Relatório, checkpoint e commits pequenos.

## 13. Relatório e evidências
Classificar achados como CONFIRMADO, FORTEMENTE INDICADO, HIPÓTESE ou DESCONHECIDO. Separar PLANEJADO, IMPLEMENTADO, TESTADO, BLOQUEADO e CONCLUÍDO. Registrar comandos e resultados literais no relatório e no checkpoint.

## 14. Encerramento
Ao concluir a Fase 0, entregar relatório com evidências e parar. Não iniciar Fase 1 sem autorização explícita. Se houver bloqueio, concluir tarefas independentes e indicar a menor ação necessária para resolver o impedimento.
