# ProjetoRPG — MMORPG 3D online

> **Objetivo:** construir, por etapas verificáveis, um MMORPG 3D online com personagens persistentes, classes, combate em tempo real, monstros, habilidades, itens, inventário, mapas, progressão, missões, recursos sociais e PvP.
>
> **Regra central para qualquer Agent:** este README, junto dos documentos oficiais existentes no repositório, define o que deve ser construído e em que ordem. Leia antes de alterar código, siga o roadmap fase por fase e mantenha o checkpoint atualizado.

---

## 1. Instruções obrigatórias para Agents

Este projeto deve ser desenvolvido como software real, não como uma coleção de telas demonstrativas ou um protótipo que apenas aparenta funcionar.

Antes de iniciar qualquer tarefa:

1. Leia este README integralmente.
2. Inspecione a árvore completa do repositório e identifique os arquivos de código, conteúdo, testes, ferramentas e documentação.
3. Leia todos os documentos existentes que sejam relevantes, especialmente `CHECKPOINT.md`, `02_ARQUITETURA.md`, documentos de visão, regras, roadmap, instruções de agentes e decisões em `docs/adr/`. Não presuma que os exemplos de caminhos deste README existem: confirme-os na árvore real.
4. Compare o que está documentado com o código e os recursos que realmente existem. Um arquivo, uma classe, uma cena ou um botão não provam que a funcionalidade está completa.
5. Determine a fase atual a partir do `CHECKPOINT.md` e dos artefatos/testes existentes. Continue do próximo passo real pendente; não reinicie o projeto sem necessidade.
6. Identifique requisitos contraditórios, decisões não confirmadas, dependências e riscos antes de fazer alterações estruturais.

### Regras de execução

- Siga as fases **0 a 14, na ordem**, respeitando suas dependências. Não pule uma fase porque uma tela já existe ou porque é mais fácil começar outra.
- Use os documentos existentes como referência complementar. Este README descreve o destino funcional e a ordem geral; documentos especializados podem conter detalhes adicionais e critérios mais específicos.
- Se houver conflito entre documentos, não escolha silenciosamente. Identifique o conflito, avalie o estado real do código, registre uma decisão justificada e atualize os documentos afetados mantendo um histórico.
- Preserve trabalho válido, alterações do usuário e funcionalidades existentes. Não apague ou substitua código, assets, dados ou configurações sem compreender o impacto.
- Não introduza dependências, engines, serviços pagos ou migrações tecnológicas importantes sem avaliar custo, compatibilidade, licença e impacto. Registre decisões arquiteturais relevantes em ADR.
- Prefira entregas pequenas, executáveis e testáveis. Faça build e testes frequentemente.
- Não use mocks, placeholders ou dados fixos como substitutos de funcionalidades de produção. Mocks são aceitáveis apenas em testes e protótipos claramente identificados.
- Não declare build, teste, integração, persistência, multiplayer ou fase como concluídos sem executar as verificações aplicáveis e registrar os resultados.
- Não esconda falhas, testes ignorados, recursos incompletos ou limitações. Liste-os claramente.
- Não pare após produzir um plano se houver trabalho implementável na fase atual. Faça a próxima tarefa definida pelo checkpoint e prossiga de forma autônoma dentro do escopo.
- Se uma dependência externa ou uma limitação real bloquear o trabalho, conclua tudo o que puder sem contornar segurança ou destruir dados; registre o bloqueio, evidências e o próximo passo exato.

### Atualização obrigatória da documentação

Ao concluir **cada tarefa significativa**:

- Atualize `CHECKPOINT.md`: fase atual, tarefas concluídas, testes e resultados, problemas abertos e próximo passo exato.
- Acrescente ao histórico resumido; não apague entradas históricas.
- Atualize roadmap, arquitetura, instruções, changelog e ADRs quando forem afetados.
- Registre comandos reproduzíveis para compilar, testar, executar cliente/servidor e validar conteúdo.
- Mantenha os estados das fases honestos: **NÃO INICIADA**, **EM ANDAMENTO**, **BLOQUEADA** ou **CONCLUÍDA**.
- Uma fase só pode ser marcada como concluída quando seus critérios de aceite forem satisfeitos e houver evidência. Uma compilação bem-sucedida, sozinha, não prova que os recursos de gameplay funcionam.

Ao retomar o projeto, leia o checkpoint e valide se o estado descrito corresponde ao repositório atual. Se estiver desatualizado, corrija-o com base em evidências.

---

## 2. Visão do jogo e resultado esperado

Criar um MMORPG de fantasia em 3D, com atmosfera de fantasia sombria, cenários ricos, personagens e monstros reconhecíveis, combate responsivo e interface legível. O mundo deve permitir que vários jogadores compartilhem mapas, encontrem monstros, evoluam seus personagens, obtenham equipamentos, interajam e disputem combates.

O resultado final precisa ser um jogo realmente executável, com cliente e servidor integrados, contas e personagens persistentes, conteúdo validado, sistemas de jogo conectados entre si, testes automatizados e instruções de instalação e execução.

### Princípios do produto

- **Gameplay real:** controles, ataques, atributos, itens, habilidades e progressão devem alterar o estado do jogo.
- **Servidor autoritativo:** decisões importantes de movimento, combate, recompensas, inventário, economia e persistência são validadas pelo servidor.
- **Persistência:** progresso válido deve sobreviver ao fechamento e à reabertura do cliente e do servidor.
- **Multiplayer verdadeiro:** jogadores conectados compartilham o mundo e recebem atualizações de outros jogadores; não basta mostrar personagens locais falsos.
- **Conteúdo orientado a dados:** itens, monstros, habilidades, mapas, missões, drops e demais catálogos devem ser definidos em dados validados, evitando duplicação desnecessária no código.
- **Testabilidade:** regras importantes devem poder ser verificadas por testes automatizados, incluindo testes sem interface gráfica quando aplicável.
- **Evolução incremental:** cada fase deve produzir uma base utilizável para a fase seguinte.

---

## 3. Arquitetura e tecnologias

O documento `02_ARQUITETURA.md` apresenta uma proposta inicial: cliente Godot 4 com C#, servidor dedicado em .NET/C#, PostgreSQL (com SQLite possível no desenvolvimento), biblioteca compartilhada, comunicação de rede, conteúdo em JSON/YAML validado e testes automatizados.

Essa proposta **não deve ser tratada como decisão final antes da confirmação prevista na Fase 0**. O Agent deve avaliar rapidamente a adequação técnica, as versões suportadas, as ferramentas disponíveis no ambiente, os requisitos de exportação e o custo de manutenção. Registrar a decisão em `docs/adr/ADR-001-stack.md` (ou no local oficial já adotado pelo repositório) e atualizar a arquitetura. Não desperdice uma sessão inteira comparando alternativas: decida com evidências suficientes e avance.

### Separação lógica recomendada

- **Shared:** tipos de domínio, identificadores, protocolo, validação e fórmulas que precisem ser compartilhadas.
- **Server:** sessões, autenticação, mundo, entidades, combate, inventário, missões, economia, persistência, ferramentas administrativas e observabilidade.
- **Client:** renderização 3D, controles, interface, animações, áudio, apresentação de entidades e comunicação com o servidor.
- **Content:** catálogos e definições de mapas, classes, itens, monstros, habilidades, NPCs, quests e drops.
- **Tools:** validador de conteúdo, ferramentas de desenvolvimento e bots de carga, conforme as fases.
- **Tests:** testes unitários, integração, simulação de regras e testes de fluxo cliente-servidor.

A estrutura exata pode ser adaptada à implementação existente, mas responsabilidades devem continuar separadas e claras.

### Princípios técnicos

- O servidor valida ações importantes; nunca confia em dano, moedas, drops ou inventário enviados pelo cliente.
- Definir limites de frequência, validação de entradas, tratamento de desconexões e erros de rede.
- Usar identificadores estáveis para conteúdo, versionar protocolo e conteúdo e validar dados no carregamento.
- Usar migrações versionadas para mudanças de banco; nunca apagar dados de jogadores como atalho para corrigir esquema.
- Implementar logs estruturados e métricas úteis desde cedo.
- Definir frequência de atualização do mundo adequada à arquitetura; a proposta inicial de 20 Hz deve ser medida e ajustada por testes, não considerada uma constante inquestionável.
- Implementar sincronização eficiente de entidades próximas (AOI/área de interesse) quando o mundo e a escala do multiplayer exigirem.
- Não incluir senhas, tokens, chaves ou segredos em código, logs, commits ou arquivos públicos.

---

## 4. Controles obrigatórios do jogador

O controle de gameplay deve ser baseado em cliques do mouse. As teclas de atalho indicadas abaixo podem abrir interfaces, mas não devem movimentar o personagem nem executar ações de combate.

| Entrada | Comportamento obrigatório |
|---|---|
| Clique esquerdo no chão | O personagem se desloca até o ponto clicado, respeitando navegação, colisões e obstáculos. |
| Clique esquerdo em um alvo interativo | Seleciona ou interage com o alvo conforme o contexto, sem criar ambiguidades com a movimentação. |
| Clique direito em inimigo válido | Ordena ataque ao alvo; o servidor valida alcance, linha de visão quando aplicável, tempo de ataque, estado do personagem e demais regras. |
| Clique direito sem alvo válido | Não dispara ataques arbitrários; comportamento consistente e documentado. |
| `I` | Abre/fecha o inventário. |
| `P` | Abre/fecha o painel do personagem. |
| `M` | Abre/fecha a lista de mapas. |
| `W`, `A`, `S`, `D`, Espaço | **Não executam funções de gameplay.** Não movimentam, atacam, esquivam, pulam nem ativam habilidades. |

Os atalhos I, P e M são exceções de interface expressamente permitidas. Outras teclas não devem ganhar funções de gameplay sem alteração explícita dos requisitos. O Agent deve pesquisar e remover mapeamentos antigos de teclado, inclusive ações herdadas da engine, scripts, configurações de input e atalhos de depuração acessíveis ao jogador.

### Aceite dos controles

- Clique esquerdo no chão produz movimento real até o destino válido.
- Obstáculos e limites do cenário são respeitados.
- Clique direito em inimigo executa o fluxo de ataque real e respeita as regras do servidor.
- W/A/S/D/Espaço não provocam ações de gameplay.
- I, P e M abrem as interfaces corretas.
- Seleção de alvo, movimento, combate e interfaces não entram em conflito em situações comuns.

---

## 5. Interface, inventário e personagem

A interface deve ter identidade visual consistente, hierarquia clara, textos legíveis e estados visuais para seleção, bloqueio, carregamento, erro e sucesso. Deve funcionar nas resoluções suportadas e não pode esconder informações essenciais.

### 5.1 Inventário — tecla I

Criar inventário de verdade, não apenas uma janela decorativa. Ele deve incluir, conforme a progressão das fases:

- Grade de espaços com ícones, quantidades e estados.
- Seleção e inspeção de itens, incluindo nome, descrição, raridade e atributos pertinentes.
- Equipar e desequipar itens compatíveis.
- Empilhar e separar itens empilháveis, quando aplicável.
- Regras de capacidade e de slots.
- Feedback de operação bem-sucedida ou recusada.
- Atualização coerente após loot, compra, venda, uso, troca, equipar e desequipar.
- Persistência no servidor.
- Validação contra duplicação, alteração de quantidades pelo cliente e outras formas de exploração.

**Referência visual:** se o usuário fornecer uma imagem de referência do inventário, use-a para orientar composição, organização, proporções e estilo, sem copiar recursos protegidos indevidamente. Na ausência da imagem, construa uma interface original e coerente com o tema do jogo.

### 5.2 Painel do personagem — tecla P

Exibir, com valores provenientes do estado real do personagem:

- Nome e classe.
- Nível e experiência atual; progresso até o próximo nível.
- Pontos de atributo disponíveis.
- Nível da conta, se o sistema de conta estiver definido e implementado.
- Status VIP somente se existir um sistema VIP documentado e funcional; não inventar benefícios pagos sem decisão de produto.
- Mapa/localização atual.
- Atributos distribuíveis: **Fúria, Defesa, Mana, Agilidade e Sabedoria**.
- Estatísticas derivadas relevantes, como vida, mana máxima, ataque, defesa e velocidade, conforme as regras do jogo.

O jogador deve conseguir distribuir pontos válidos. O servidor valida a quantidade disponível, limites, elegibilidade e resultado. A interface atualiza apenas após confirmação do estado autoritativo.

### 5.3 Atributos obrigatórios

Os cinco atributos solicitados precisam ter impacto de gameplay mensurável:

- **Fúria:** deve influenciar o poder ofensivo conforme fórmulas documentadas.
- **Defesa:** deve influenciar mitigação ou resistência a dano conforme regras documentadas.
- **Mana:** deve influenciar a capacidade de usar habilidades e/ou a reserva máxima de mana; a regra exata deve ser consistente.
- **Agilidade:** deve influenciar parâmetros apropriados, como velocidade, precisão, evasão ou velocidade de ataque, sem criar vantagens incoerentes.
- **Sabedoria:** deve influenciar habilidades mágicas, eficiência de mana, poder mágico ou outro efeito claramente documentado.

Esses efeitos são requisitos de produto, não fórmulas numéricas pré-definidas. Antes de implementá-los, compare-os com os atributos FOR, AGI, VIT, ENE, CMD ou quaisquer atributos já presentes nos documentos/código. Registre o mapeamento ou a substituição em uma decisão documentada. Não mantenha dois sistemas de atributos paralelos. Defina fórmulas, limites, efeitos por classe e testes para provar que cada atributo altera o resultado esperado.

---

## 6. Mapas e teleporte — tecla M

A lista de mapas deve apresentar os mapas disponíveis, seus requisitos e o custo de teleporte em **Coins**.

Cada mapa deve possuir dados consistentes, incluindo, conforme necessário:

- Identificador estável, nome e descrição.
- Requisito de nível e outros requisitos de acesso.
- Custo de teleporte em Coins.
- Ponto de chegada válido.
- Regras de acesso, instância e/ou região.
- Monstros, NPCs, recursos e conteúdo associados.
- Estado de disponibilidade.

O teleporte deve validar no servidor o mapa de destino, nível, custo, saldo, estado do personagem e regras de acesso. O custo deve ser cobrado de forma atômica: não cobrar sem teletransportar e não teletransportar sem cobrar quando o custo for obrigatório. Mostrar claramente por que uma viagem foi recusada.

O catálogo não pode ser preenchido com mapas falsos que não existam no mundo. Se o mapa ainda não estiver implementado, identificar seu estado como planejado/bloqueado e impedir que seja apresentado como destino funcional.

---

## 7. Tela de carregamento, login e criação de personagens

### 7.1 Tela de carregamento

Criar uma tela de carregamento polida com arte original de fantasia sombria mostrando várias classes em combate contra monstros e entre si, dentro de um castelo em ruínas e em chamas. Usar recursos licenciados adequadamente ou assets originais; não alegar que uma imagem é uma renderização real do jogo se for apenas arte conceitual.

A tela deve mostrar progresso ou estado real de carregamento sempre que tecnicamente possível, com tratamento de falha, timeout e possibilidade de tentar novamente. Evitar uma barra de progresso fictícia que avance sem relação com o processo.

### 7.2 Login e contas

Após a etapa inicial de carregamento, exibir a tela de login com campos apropriados, validação e mensagens claras. Implementar:

- Cadastro e autenticação de contas, conforme a arquitetura aprovada.
- Senhas armazenadas com hash apropriado e salt por biblioteca reconhecida; nunca em texto puro.
- Validação de entrada, limites de tentativas e proteção básica contra abuso.
- Sessões/tokens tratados de forma segura, com expiração e revogação conforme a arquitetura.
- Mensagens de erro que não revelem informações sensíveis.
- Persistência no servidor e tratamento de indisponibilidade de rede.
- Fluxo de logout e reconexão consistente.

Não adicionar senhas padrão ou credenciais reais ao repositório. Recursos como recuperação de senha, verificação de e-mail e autenticação adicional devem ser definidos no roadmap ou numa decisão explícita se não houver provedor/infraestrutura disponível.

### 7.3 Lobby de criação de personagem

Depois do login, permitir criar personagem em um lobby visual. O jogador deve poder:

- Escolher entre as classes efetivamente implementadas.
- Visualizar imagem/modelo da classe e uma breve descrição.
- Consultar atributos iniciais, estilo de combate e papel esperado.
- Informar um nome e receber validação de formato, tamanho, caracteres e unicidade.
- Confirmar a criação e receber resposta do servidor.
- Ver mensagens úteis quando o nome estiver ocupado ou a criação for recusada.

A criação deve persistir e não gerar personagens duplicados por cliques repetidos ou falhas de rede. As classes exibidas precisam corresponder a classes realmente configuradas no conteúdo.

### 7.4 Seleção de personagens

Depois de criar um personagem, apresentar a tela de seleção com os personagens da conta organizados lado a lado, com modelo/imagem, nome, classe, nível e demais informações relevantes. Permitir selecionar e entrar no mundo com o personagem escolhido. Respeitar limites de personagens por conta se esse limite for definido.

Não usar uma lista estática de personagens de demonstração como substituto da consulta ao servidor.

---

## 8. Sistemas centrais de MMORPG

Todos os sistemas abaixo devem ser integrados entre si e evoluir conforme as fases do roadmap. Não é necessário implementar tudo na primeira entrega.

### Mundo e movimentação

- Mapas 3D navegáveis, colisões, limites, pontos de entrada e saída.
- Navegação por clique esquerdo, com destino válido e feedback visual.
- Spawn de jogadores, monstros e NPCs baseado em conteúdo.
- Sincronização entre jogadores conectados e tratamento de desconexões.
- Transição de mapas e restauração de estado ao reconectar.

### Classes e progressão

- Classes distintas com identidade visual, estilo de combate e habilidades.
- Nível, experiência, recompensas e curva de progressão configuráveis.
- Pontos de atributo com validação autoritativa.
- Fórmulas de estatísticas e crescimento documentadas e testadas.
- Balanceamento orientado por dados, sem números críticos espalhados por scripts.

### Combate e habilidades

- Alvos, alcance, cooldowns, animações, feedback de acerto e dano.
- Ataques básicos acionados por clique direito.
- Habilidades ativas/passivas, custos, requisitos, cooldowns e efeitos.
- Estados como morte, atordoamento, buffs/debuffs e invulnerabilidade apenas quando definidos e implementados.
- Cálculo autoritativo no servidor, com validação de distância, tempo e estado.
- Fórmulas determinísticas e testes para dano, mitigação, crítico, recursos e efeitos.
- Feedback visual e sonoro sem substituir a validação real.

### Monstros, NPCs e loot

- Dados de monstros com atributos, comportamento, ataques, recompensas e locais de spawn.
- IA básica funcional: detectar, perseguir, atacar, retornar quando apropriado e morrer.
- NPCs com interação contextual e serviços implementados.
- Drops calculados no servidor, com tabelas de probabilidade configuráveis.
- Experiência e recompensas concedidas uma única vez e persistidas corretamente.
- Prevenção de duplicação de loot e exploração de recompensas.

### Itens, equipamento e economia

- Catálogo de itens com IDs estáveis, tipo, raridade, ícone, descrição e propriedades.
- Equipamentos com slots e requisitos compatíveis com classes/regras.
- Inventário, loot, consumo, lojas e moedas com operações validadas no servidor.
- Comprar, vender e usar itens com tratamento atômico e feedback claro.
- Persistência e proteção contra duplicação, saldo negativo e alterações forjadas.
- Refinamento e crafting apenas conforme as regras e os dados definidos na fase correspondente.

### Missões, NPCs e bosses

- Objetivos e condições definidos em dados.
- Progresso rastreável e persistente.
- Recompensas concedidas uma única vez, com validação no servidor.
- NPCs e bosses com comportamentos e recompensas próprios.
- Regras de reset, repetição e pré-requisitos documentadas.

### Sistemas sociais

Conforme as fases, construir chat, grupo/party, guildas, troca entre jogadores e demais recursos sociais definidos no roadmap. Validar permissões, participantes, estados e operações econômicas no servidor. Trocas devem ser seguras contra alterações de última hora, desconexões e duplicação de itens.

### PvP, PK, rankings e eventos

- PvP com regras explícitas de alvo, dano, zonas e estados.
- PK e penalidades apenas quando formalmente definidos.
- Rankings calculados a partir de dados confiáveis.
- Eventos e cerco com regras, participantes, duração e recompensas configuráveis.
- Não criar sistemas meramente decorativos nem expor comandos administrativos inseguros.

---

## 9. Multiplayer, banco e segurança

O servidor deve ser a autoridade para estado persistente e regras que possam afetar justiça, progressão ou economia. O cliente envia intenções; o servidor valida e responde com resultados.

### Persistência

Persistir, conforme a arquitetura e as fases:

- Contas e credenciais protegidas.
- Personagens, classe, nível, experiência e atributos.
- Localização e estado necessário à reconexão.
- Inventário, equipamentos, moedas e progresso.
- Missões, configurações e dados sociais relevantes.

Definir migrações versionadas, integridade referencial, transações para operações críticas, backups e procedimentos de recuperação apropriados ao estágio do projeto. Testar reinício do servidor e reconexão para verificar que os dados permanecem íntegros.

### Segurança e confiabilidade

- Validar toda entrada recebida pela rede.
- Impor limites de frequência e tamanho das mensagens.
- Tratar desconexões, timeouts, pacotes inválidos e falhas de banco.
- Evitar confiar no relógio, no dano ou nos saldos calculados pelo cliente.
- Prevenir repetição indevida de requisições críticas, duplicação de recompensas e transações parciais.
- Não registrar senhas, tokens ou dados secretos.
- Não abrir portas ou publicar servidores/credenciais sem autorização e documentação.
- Testar abuso básico, falhas de rede e operações concorrentes antes do lançamento.

---

## 10. Arte 3D, animação e áudio

A qualidade visual deve evoluir em conjunto com o gameplay, sem bloquear a entrega de uma base funcional.

- Personagens e classes visualmente distinguíveis.
- Monstros e NPCs com silhuetas e animações adequadas.
- Animações de idle, movimento, ataque, habilidade, dano e morte, conforme o conteúdo disponível.
- Cenários com iluminação, materiais, colisões e navegação utilizáveis.
- Efeitos de impacto, habilidades, teleporte e loot, com legibilidade visual.
- Áudio para interface, ambiente, combate e ações importantes.
- Interface consistente com o mundo e o tema de fantasia sombria.
- Otimização de polígonos, texturas, materiais, efeitos e carregamento conforme as medições.

Use assets próprios, gerados para o projeto ou com licenças compatíveis. Registre origem e licença de assets externos. Não afirme que existem modelos, animações, efeitos ou sons que não estejam realmente presentes e integrados. Se algum recurso artístico avançado não puder ser produzido na fase atual, use uma alternativa provisória claramente identificada, mantenha o jogo executável e registre a pendência.

---

## 11. Roadmap obrigatório — Fases 0 a 14

O `CHECKPOINT.md` existente define a sequência de 15 fases abaixo. Preserve seus nomes e sua ordem; detalhe tarefas conforme a auditoria do código e dos demais documentos. A lista a seguir descreve os resultados esperados e não autoriza pular fases.

### Fase 0 — Preparação

**Objetivo:** estabelecer uma base técnica reproduzível.

- Auditar repositório, documentos, ambiente e ferramentas instaladas.
- Confirmar a stack proposta em `02_ARQUITETURA.md` e registrar ADR-001.
- Definir estrutura de diretórios e convenções de código.
- Preparar um processo de build/teste reproduzível.
- Criar um primeiro teste executável do servidor e do cliente.
- Registrar versões, comandos de build, testes e execução.
- Atualizar `CHECKPOINT.md` com fatos verificados.

**Aceite:** decisão arquitetural registrada; estrutura e scripts funcionam; cliente e servidor têm um teste inicial reproduzível; documentação indica como executar.

### Fase 1 — Fundação do servidor

**Objetivo:** estabelecer o núcleo confiável do servidor.

- Inicializar servidor e biblioteca compartilhada.
- Implementar configuração, logging, tratamento de erros e ciclo principal.
- Definir protocolo inicial, versões e validação de mensagens.
- Criar acesso ao banco, esquema inicial e migrações versionadas.
- Estabelecer contas/sessões na medida necessária para as fases seguintes.
- Implementar testes unitários e de integração básicos.

**Aceite:** servidor inicia de forma reproduzível; banco e migrações funcionam; testes básicos passam; entradas inválidas são tratadas.

### Fase 2 — Cliente base e login

**Objetivo:** conectar o cliente ao servidor e habilitar acesso de conta.

- Inicializar o cliente e sua estrutura de cenas/telas.
- Implementar tela de carregamento e login.
- Conectar autenticação ao servidor.
- Tratar carregamento, erro, desconexão e nova tentativa.
- Criar os primeiros testes do fluxo cliente-servidor.

**Aceite:** cliente conecta-se ao servidor e autentica uma conta de teste por fluxo real; erros são apresentados de forma compreensível; credenciais não são expostas.

### Fase 3 — Mundo e movimento

**Objetivo:** permitir entrar num mapa e controlar o personagem.

- Criar a primeira área 3D navegável.
- Implementar entidades, spawn, colisões e navegação.
- Implementar movimento por clique esquerdo.
- Remover movimento por W/A/S/D/Espaço.
- Sincronizar movimento entre clientes conectados.
- Implementar entrada, saída e transição básica de mundo.

**Aceite:** movimento por clique funciona; obstáculos são respeitados; controles de teclado proibidos não executam gameplay; dois clientes conseguem observar o movimento um do outro por sincronização real.

### Fase 4 — Combate, monstros e habilidades

**Objetivo:** criar o primeiro ciclo real de combate.

- Implementar seleção de alvo e ataque básico por clique direito.
- Criar monstros com spawn e IA básica.
- Implementar vida, dano, morte e recompensas iniciais.
- Adicionar estrutura de habilidades, cooldowns e custos.
- Validar alcance, estado e frequência de ações no servidor.
- Criar testes de fórmulas e fluxo de combate.

**Aceite:** o jogador ataca monstros reais; o servidor calcula o resultado; estados de vida/morte e recompensas funcionam; testes cobrem casos válidos e inválidos.

### Fase 5 — Itens, inventário, drops e lojas

**Objetivo:** criar o ciclo inicial de itens e economia.

- Criar catálogo de itens e validação de conteúdo.
- Implementar inventário e interface da tecla I.
- Implementar drops, coleta, equipamentos e itens consumíveis.
- Implementar lojas e transações iniciais conforme o conteúdo definido.
- Persistir itens e saldos.
- Testar operações concorrentes e prevenção de duplicação.

**Aceite:** itens podem ser obtidos e manipulados por fluxo real; inventário persiste; compras, drops e equipamentos são validados pelo servidor.

### Fase 6 — Progressão e classes

**Objetivo:** estabelecer personagens jogáveis e progressão.

- Implementar classes efetivas, criação e seleção de personagem.
- Implementar nível, experiência, curva de progressão e pontos.
- Implementar painel P com estado real.
- Definir e documentar Fúria, Defesa, Mana, Agilidade e Sabedoria, reconciliando atributos existentes.
- Implementar fórmulas, efeitos por classe e validação de distribuição.
- Persistir o progresso.

**Aceite:** criação/seleção funciona com dados persistidos; atributos alteram gameplay; progressão e painel refletem o estado real; fórmulas têm testes.

### Fase 7 — Social

**Objetivo:** habilitar interação entre jogadores.

- Implementar os recursos sociais definidos na documentação, incluindo chat e grupo/party conforme o escopo registrado.
- Definir estados, permissões, convites e tratamento de desconexões.
- Implementar testes de autorização e sincronização.

**Aceite:** recursos sociais previstos funcionam entre clientes conectados e respeitam as permissões definidas.

### Fase 8 — Refino, crafting e opções

**Objetivo:** ampliar o ciclo de equipamentos e personalização.

- Implementar refino e crafting orientados a dados.
- Definir custos, materiais, resultados, probabilidades e falhas quando aplicável.
- Implementar opções/configurações do jogador que estiverem definidas no projeto.
- Validar e persistir as operações no servidor.

**Aceite:** receitas e regras são validadas; materiais/custos são debitados corretamente; não há duplicação em falhas ou requisições repetidas.

### Fase 9 — PvP, PK e ranking

**Objetivo:** implementar combate entre jogadores com regras explícitas.

- Definir zonas e elegibilidade de PvP.
- Implementar dano entre jogadores e estados pertinentes.
- Implementar PK/penalidades apenas conforme regras aprovadas.
- Criar rankings baseados em dados confiáveis.
- Testar permissões, exploits básicos e resultados persistidos.

**Aceite:** PvP segue as regras documentadas; rankings não podem ser manipulados pelo cliente; estados e penalidades são consistentes.

### Fase 10 — Quests, NPCs e bosses

**Objetivo:** ampliar o conteúdo de mundo.

- Implementar sistema de quests orientado a dados.
- Criar objetivos, pré-requisitos, progresso, conclusão e recompensas.
- Integrar NPCs interativos e serviços previstos.
- Implementar bosses e mecânicas/recompensas definidas.
- Testar persistência e concessão única de recompensas.

**Aceite:** quests podem ser iniciadas e concluídas; progresso persiste; recompensas não são duplicadas; NPCs e bosses têm fluxos funcionais.

### Fase 11 — Eventos

**Objetivo:** implementar eventos do mundo e eventos instanciados previstos no projeto.

- Definir regras, calendário/ativação, participantes e condições de conclusão.
- Implementar recompensas e encerramento seguro.
- Integrar cerco ou outros eventos de maior escala se estiverem definidos no escopo oficial.
- Testar concorrência, desconexões e estados de evento.

**Aceite:** eventos possuem ciclo de vida reproduzível e suas regras/recompensas são validadas no servidor.

### Fase 12 — Ferramentas de conteúdo

**Objetivo:** tornar o conteúdo sustentável e validável.

- Implementar ou consolidar o validador de conteúdo.
- Validar referências, IDs, schemas, faixas numéricas e dependências.
- Criar ferramenta de edição de conteúdo somente conforme o escopo e as tecnologias aprovadas.
- Documentar o fluxo para adicionar mapas, itens, monstros, skills, NPCs e quests.
- Adicionar exemplos e testes de conteúdo inválido.

**Aceite:** dados inválidos são detectados antes de serem usados; o fluxo de criação de conteúdo é documentado e repetível.

### Fase 13 — Performance, segurança e QA

**Objetivo:** preparar o jogo para testes ampliados e reduzir riscos.

- Executar testes de carga e simulação de jogadores conforme a capacidade do ambiente.
- Medir ticks do servidor, rede, uso de memória, banco e renderização.
- Corrigir gargalos demonstrados por medições.
- Revisar validação de entrada, rate limits, autenticação, economia e permissões.
- Testar desconexões, reconexões, concorrência, persistência e recuperação.
- Corrigir falhas críticas e documentar riscos restantes.

**Aceite:** testes e métricas têm resultados registrados; falhas críticas conhecidas foram corrigidas ou estão claramente bloqueadas e documentadas; não há alegação de capacidade sem evidência.

### Fase 14 — Polimento e lançamento

**Objetivo:** produzir uma versão utilizável, documentada e reproduzível.

- Polir interface, animações, áudio, feedback e acessibilidade básica.
- Verificar fluxos completos de conta, criação/seleção, mundo, combate, inventário, progressão e demais sistemas previstos.
- Validar configuração de cliente e servidor para o ambiente de destino.
- Preparar instruções de instalação, build, execução, backup e solução de problemas.
- Documentar limitações conhecidas, dependências e licenças de assets.
- Produzir artefatos de build somente para plataformas realmente suportadas e testadas.

**Aceite:** instalação e execução podem ser reproduzidas seguindo a documentação; testes de regressão relevantes passam; os recursos ausentes e limitações são explicitamente listados.

---

## 12. Critérios gerais de qualidade e conclusão

Uma funcionalidade é considerada concluída quando:

1. Está implementada e conectada ao fluxo real do jogo.
2. Tem validação de entradas e regras pertinentes.
3. Atualiza o estado correto no cliente e/ou servidor.
4. Persiste dados quando necessário.
5. Tem feedback de sucesso, falha e estados inválidos.
6. Possui testes adequados ou uma verificação manual reproduzível quando automação não for viável.
7. Não quebra funcionalidades existentes.
8. Sua documentação está atualizada.

Uma fase é concluída apenas quando todos os seus critérios de aceite foram verificados. Se um item estiver bloqueado por dependência externa, marque a fase como **BLOQUEADA** ou **EM ANDAMENTO**, explique a evidência e prossiga apenas com tarefas independentes seguras.

### Testes mínimos a incorporar progressivamente

- Build limpo de cada componente relevante.
- Testes unitários de regras e fórmulas.
- Testes de integração com banco e migrações.
- Testes de protocolo e autenticação.
- Testes de fluxo cliente-servidor.
- Testes de persistência após reinício.
- Testes com dois ou mais clientes para recursos multiplayer.
- Testes de entradas inválidas, ações não autorizadas e operações repetidas.
- Testes de regressão dos controles por mouse e atalhos I/P/M.

Os comandos exatos devem ser descobertos, confirmados e registrados durante a Fase 0; não inventar comandos nem declarar que foram executados se não foram.

---

## 13. Checkpoint, histórico e passagem de trabalho

O arquivo `CHECKPOINT.md` é a referência operacional para retomar o trabalho. Deve conter, no mínimo:

- Fase atual e seu estado.
- Última tarefa concluída e evidência.
- Data da atualização.
- Próximo passo exato, pequeno e executável.
- Comandos reais de build, testes e execução.
- Ambiente e versões verificadas.
- Resultados dos testes executados.
- Problemas abertos, bloqueios e suposições.
- Histórico resumido append-only.

Ao finalizar uma sessão, deixe o repositório em estado compreensível para outro Agent. Evite terminar com uma tarefa vaga como “continuar desenvolvimento”. Especifique o arquivo/módulo a trabalhar, o resultado esperado e como validar.

---

## 14. Formato obrigatório do relatório do Agent

Ao concluir uma tarefa ou interromper por bloqueio, apresente:

1. **Fase e objetivo:** fase atual e tarefa tratada.
2. **Implementado:** arquivos e comportamento efetivamente criados/alterados.
3. **Validação:** comandos executados e resultados reais, incluindo falhas.
4. **Integração:** como o recurso se conecta aos sistemas existentes.
5. **Documentação:** arquivos atualizados.
6. **Pendências/bloqueios:** o que ainda não está pronto e por quê.
7. **Próximo passo exato:** a próxima tarefa de acordo com o roadmap e checkpoint.

Não confunda “planejado”, “iniciado”, “implementado”, “testado” e “concluído”. Não apresente uma lista de intenções como se fosse uma entrega pronta.

---

## 15. Primeira ação ao receber este README

O Agent deve começar auditando o repositório e a documentação, confirmando a fase atual registrada em `CHECKPOINT.md` e comparando-a com o estado real dos arquivos e testes. Como o checkpoint existente aponta para a **Fase 0 — Preparação**, verifique se isso continua verdadeiro antes de agir.

Se a Fase 0 ainda estiver incompleta, execute o próximo passo documentado: avaliar e confirmar a arquitetura com ADR-001, estabelecer a estrutura necessária e preparar build/teste inicial de cliente e servidor. Não comece pelas telas ou por sistemas avançados antes de estabelecer essa base.

Depois, implemente, valide e documente o trabalho de forma incremental, seguindo o roadmap até a Fase 14. O objetivo é entregar um MMORPG real, e não apenas um plano, uma demonstração visual ou um conjunto de arquivos sem integração.
