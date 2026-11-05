# ProjetoRPG ÔÇö MMORPG 3D online

> **Objetivo:** construir, por etapas verific├íveis, um MMORPG 3D online com personagens persistentes, classes, combate em tempo real, monstros, habilidades, itens, invent├írio, mapas, progress├úo, miss├Áes, recursos sociais e PvP.
>
> **Regra central para qualquer Agent:** este README, junto dos documentos oficiais existentes no reposit├│rio, define o que deve ser constru├¡do e em que ordem. Leia antes de alterar c├│digo, siga o roadmap fase por fase e mantenha o checkpoint atualizado.

---

## 1. Instru├º├Áes obrigat├│rias para Agents

Este projeto deve ser desenvolvido como software real, n├úo como uma cole├º├úo de telas demonstrativas ou um prot├│tipo que apenas aparenta funcionar.

Antes de iniciar qualquer tarefa:

1. Leia este README integralmente.
2. Inspecione a ├írvore completa do reposit├│rio e identifique os arquivos de c├│digo, conte├║do, testes, ferramentas e documenta├º├úo.
3. Leia todos os documentos existentes que sejam relevantes, especialmente `CHECKPOINT.md`, `02_ARQUITETURA.md`, documentos de vis├úo, regras, roadmap, instru├º├Áes de agentes e decis├Áes em `docs/adr/`. N├úo presuma que os exemplos de caminhos deste README existem: confirme-os na ├írvore real.
4. Compare o que est├í documentado com o c├│digo e os recursos que realmente existem. Um arquivo, uma classe, uma cena ou um bot├úo n├úo provam que a funcionalidade est├í completa.
5. Determine a fase atual a partir do `CHECKPOINT.md` e dos artefatos/testes existentes. Continue do pr├│ximo passo real pendente; n├úo reinicie o projeto sem necessidade.
6. Identifique requisitos contradit├│rios, decis├Áes n├úo confirmadas, depend├¬ncias e riscos antes de fazer altera├º├Áes estruturais.

### Regras de execu├º├úo

- Siga as fases **0 a 14, na ordem**, respeitando suas depend├¬ncias. N├úo pule uma fase porque uma tela j├í existe ou porque ├® mais f├ícil come├ºar outra.
- Use os documentos existentes como refer├¬ncia complementar. Este README descreve o destino funcional e a ordem geral; documentos especializados podem conter detalhes adicionais e crit├®rios mais espec├¡ficos.
- Se houver conflito entre documentos, n├úo escolha silenciosamente. Identifique o conflito, avalie o estado real do c├│digo, registre uma decis├úo justificada e atualize os documentos afetados mantendo um hist├│rico.
- Preserve trabalho v├ílido, altera├º├Áes do usu├írio e funcionalidades existentes. N├úo apague ou substitua c├│digo, assets, dados ou configura├º├Áes sem compreender o impacto.
- N├úo introduza depend├¬ncias, engines, servi├ºos pagos ou migra├º├Áes tecnol├│gicas importantes sem avaliar custo, compatibilidade, licen├ºa e impacto. Registre decis├Áes arquiteturais relevantes em ADR.
- Prefira entregas pequenas, execut├íveis e test├íveis. Fa├ºa build e testes frequentemente.
- N├úo use mocks, placeholders ou dados fixos como substitutos de funcionalidades de produ├º├úo. Mocks s├úo aceit├íveis apenas em testes e prot├│tipos claramente identificados.
- N├úo declare build, teste, integra├º├úo, persist├¬ncia, multiplayer ou fase como conclu├¡dos sem executar as verifica├º├Áes aplic├íveis e registrar os resultados.
- N├úo esconda falhas, testes ignorados, recursos incompletos ou limita├º├Áes. Liste-os claramente.
- N├úo pare ap├│s produzir um plano se houver trabalho implement├ível na fase atual. Fa├ºa a pr├│xima tarefa definida pelo checkpoint e prossiga de forma aut├┤noma dentro do escopo.
- Se uma depend├¬ncia externa ou uma limita├º├úo real bloquear o trabalho, conclua tudo o que puder sem contornar seguran├ºa ou destruir dados; registre o bloqueio, evid├¬ncias e o pr├│ximo passo exato.

### Atualiza├º├úo obrigat├│ria da documenta├º├úo

Ao concluir **cada tarefa significativa**:

- Atualize `CHECKPOINT.md`: fase atual, tarefas conclu├¡das, testes e resultados, problemas abertos e pr├│ximo passo exato.
- Acrescente ao hist├│rico resumido; n├úo apague entradas hist├│ricas.
- Atualize roadmap, arquitetura, instru├º├Áes, changelog e ADRs quando forem afetados.
- Registre comandos reproduz├¡veis para compilar, testar, executar cliente/servidor e validar conte├║do.
- Mantenha os estados das fases honestos: **N├âO INICIADA**, **EM ANDAMENTO**, **BLOQUEADA** ou **CONCLU├ìDA**.
- Uma fase s├│ pode ser marcada como conclu├¡da quando seus crit├®rios de aceite forem satisfeitos e houver evid├¬ncia. Uma compila├º├úo bem-sucedida, sozinha, n├úo prova que os recursos de gameplay funcionam.

Ao retomar o projeto, leia o checkpoint e valide se o estado descrito corresponde ao reposit├│rio atual. Se estiver desatualizado, corrija-o com base em evid├¬ncias.

---

## 2. Vis├úo do jogo e resultado esperado

Criar um MMORPG de fantasia em 3D, com atmosfera de fantasia sombria, cen├írios ricos, personagens e monstros reconhec├¡veis, combate responsivo e interface leg├¡vel. O mundo deve permitir que v├írios jogadores compartilhem mapas, encontrem monstros, evoluam seus personagens, obtenham equipamentos, interajam e disputem combates.

O resultado final precisa ser um jogo realmente execut├ível, com cliente e servidor integrados, contas e personagens persistentes, conte├║do validado, sistemas de jogo conectados entre si, testes automatizados e instru├º├Áes de instala├º├úo e execu├º├úo.

### Princ├¡pios do produto

- **Gameplay real:** controles, ataques, atributos, itens, habilidades e progress├úo devem alterar o estado do jogo.
- **Servidor autoritativo:** decis├Áes importantes de movimento, combate, recompensas, invent├írio, economia e persist├¬ncia s├úo validadas pelo servidor.
- **Persist├¬ncia:** progresso v├ílido deve sobreviver ao fechamento e ├á reabertura do cliente e do servidor.
- **Multiplayer verdadeiro:** jogadores conectados compartilham o mundo e recebem atualiza├º├Áes de outros jogadores; n├úo basta mostrar personagens locais falsos.
- **Conte├║do orientado a dados:** itens, monstros, habilidades, mapas, miss├Áes, drops e demais cat├ílogos devem ser definidos em dados validados, evitando duplica├º├úo desnecess├íria no c├│digo.
- **Testabilidade:** regras importantes devem poder ser verificadas por testes automatizados, incluindo testes sem interface gr├ífica quando aplic├ível.
- **Evolu├º├úo incremental:** cada fase deve produzir uma base utiliz├ível para a fase seguinte.

---

## 3. Arquitetura e tecnologias

A stack aprovada est├í em `docs/adr/ADR-001-stack.md`: Godot 4.7.2 Mono/`net8.0` cliente, ASP.NET Core/`net10.0` servidor, PostgreSQL/Npgsql, Shared sem depend├¬ncia Godot, WebSocket JSON versionado e conte├║do em JSON. A sele├º├úo foi decidida; build/compatibilidade real ainda requer valida├º├úo no ambiente-alvo.

### Separa├º├úo l├│gica recomendada

- **Shared:** tipos de dom├¡nio, identificadores, protocolo, valida├º├úo e f├│rmulas que precisem ser compartilhadas.
- **Server:** sess├Áes, autentica├º├úo, mundo, entidades, combate, invent├írio, miss├Áes, economia, persist├¬ncia, ferramentas administrativas e observabilidade.
- **Client:** renderiza├º├úo 3D, controles, interface, anima├º├Áes, ├íudio, apresenta├º├úo de entidades e comunica├º├úo com o servidor.
- **Content:** cat├ílogos e defini├º├Áes de mapas, classes, itens, monstros, habilidades, NPCs, quests e drops.
- **Tools:** validador de conte├║do, ferramentas de desenvolvimento e bots de carga, conforme as fases.
- **Tests:** testes unit├írios, integra├º├úo, simula├º├úo de regras e testes de fluxo cliente-servidor.

A estrutura exata pode ser adaptada ├á implementa├º├úo existente, mas responsabilidades devem continuar separadas e claras.

### Princ├¡pios t├®cnicos

- O servidor valida a├º├Áes importantes; nunca confia em dano, moedas, drops ou invent├írio enviados pelo cliente.
- Definir limites de frequ├¬ncia, valida├º├úo de entradas, tratamento de desconex├Áes e erros de rede.
- Usar identificadores est├íveis para conte├║do, versionar protocolo e conte├║do e validar dados no carregamento.
- Usar migra├º├Áes versionadas para mudan├ºas de banco; nunca apagar dados de jogadores como atalho para corrigir esquema.
- Implementar logs estruturados e m├®tricas ├║teis desde cedo.
- Definir frequ├¬ncia de atualiza├º├úo do mundo adequada ├á arquitetura; a proposta inicial de 20 Hz deve ser medida e ajustada por testes, n├úo considerada uma constante inquestion├ível.
- Implementar sincroniza├º├úo eficiente de entidades pr├│ximas (AOI/├írea de interesse) quando o mundo e a escala do multiplayer exigirem.
- N├úo incluir senhas, tokens, chaves ou segredos em c├│digo, logs, commits ou arquivos p├║blicos.

---

## 4. Controles obrigat├│rios do jogador

Nesta fatia Coast, o movimento ├® por WASD/setas e somente o servidor aplica posi├º├úo. Combate, clique para movimento e atalhos I/P/M n├úo fazem parte do fluxo jog├ível atual e n├úo devem ser simulados; futura mudan├ºa de controles requer escopo aprovado.

| Entrada | Comportamento obrigat├│rio |
|---|---|
| Clique esquerdo no ch├úo | N├úo implementado nesta fatia; n├úo altera posi├º├úo local. |
| Clique esquerdo em um alvo interativo | N├úo implementado nesta fatia. |
| Clique direito em inimigo v├ílido | Combate n├úo implementado nesta fatia. |
| Clique direito sem alvo v├ílido | Sem a├º├úo de gameplay nesta fatia. |
| `I` | Invent├írio fora do escopo desta fatia. |
| `P` | Painel do personagem fora do escopo desta fatia. |
| `M` | Lista/teleporte fora do escopo desta fatia. |
| `W`, `A`, `S`, `D`, setas | Movem o personagem na primeira ├írea Coast por inten├º├úo validada pelo servidor; o cliente n├úo envia coordenadas finais. |
| Espa├ºo | N├úo executa fun├º├úo de gameplay nesta etapa. |

WASD/setas s├úo o controle ├║nico desta fatia. Espa├ºo, mouse para movimento/combate, invent├írio, painel e mapa n├úo t├¬m a├º├úo implementada; n├úo inventar subsistemas secund├írios.

### Aceite dos controles

- WASD/setas produzem inten├º├Áes limitadas; s├│ o estado recebido do servidor reposiciona o ator e obst├ículos/bordas da Coast param o movimento.
- Espa├ºo n├úo provoca a├º├úo de gameplay nesta etapa.

---

## 5. Interface, invent├írio e personagem

A interface deve ter identidade visual consistente, hierarquia clara, textos leg├¡veis e estados visuais para sele├º├úo, bloqueio, carregamento, erro e sucesso. Deve funcionar nas resolu├º├Áes suportadas e n├úo pode esconder informa├º├Áes essenciais.

### 5.1 Invent├írio ÔÇö tecla I

Criar invent├írio de verdade, n├úo apenas uma janela decorativa. Ele deve incluir, conforme a progress├úo das fases:

- Grade de espa├ºos com ├¡cones, quantidades e estados.
- Sele├º├úo e inspe├º├úo de itens, incluindo nome, descri├º├úo, raridade e atributos pertinentes.
- Equipar e desequipar itens compat├¡veis.
- Empilhar e separar itens empilh├íveis, quando aplic├ível.
- Regras de capacidade e de slots.
- Feedback de opera├º├úo bem-sucedida ou recusada.
- Atualiza├º├úo coerente ap├│s loot, compra, venda, uso, troca, equipar e desequipar.
- Persist├¬ncia no servidor.
- Valida├º├úo contra duplica├º├úo, altera├º├úo de quantidades pelo cliente e outras formas de explora├º├úo.

**Refer├¬ncia visual:** se o usu├írio fornecer uma imagem de refer├¬ncia do invent├írio, use-a para orientar composi├º├úo, organiza├º├úo, propor├º├Áes e estilo, sem copiar recursos protegidos indevidamente. Na aus├¬ncia da imagem, construa uma interface original e coerente com o tema do jogo.

### 5.2 Painel do personagem ÔÇö tecla P

Exibir, com valores provenientes do estado real do personagem:

- Nome e classe.
- N├¡vel e experi├¬ncia atual; progresso at├® o pr├│ximo n├¡vel.
- Pontos de atributo dispon├¡veis.
- N├¡vel da conta, se o sistema de conta estiver definido e implementado.
- Status VIP somente se existir um sistema VIP documentado e funcional; n├úo inventar benef├¡cios pagos sem decis├úo de produto.
- Mapa/localiza├º├úo atual.
- Atributos distribu├¡veis: **F├║ria, Defesa, Mana, Agilidade e Sabedoria**.
- Estat├¡sticas derivadas relevantes, como vida, mana m├íxima, ataque, defesa e velocidade, conforme as regras do jogo.

O jogador deve conseguir distribuir pontos v├ílidos. O servidor valida a quantidade dispon├¡vel, limites, elegibilidade e resultado. A interface atualiza apenas ap├│s confirma├º├úo do estado autoritativo.

### 5.3 Atributos obrigat├│rios

Os cinco atributos solicitados precisam ter impacto de gameplay mensur├ível:

- **F├║ria:** deve influenciar o poder ofensivo conforme f├│rmulas documentadas.
- **Defesa:** deve influenciar mitiga├º├úo ou resist├¬ncia a dano conforme regras documentadas.
- **Mana:** deve influenciar a capacidade de usar habilidades e/ou a reserva m├íxima de mana; a regra exata deve ser consistente.
- **Agilidade:** deve influenciar par├ómetros apropriados, como velocidade, precis├úo, evas├úo ou velocidade de ataque, sem criar vantagens incoerentes.
- **Sabedoria:** deve influenciar habilidades m├ígicas, efici├¬ncia de mana, poder m├ígico ou outro efeito claramente documentado.

Esses efeitos s├úo requisitos de produto, n├úo f├│rmulas num├®ricas pr├®-definidas. Antes de implement├í-los, compare-os com os atributos FOR, AGI, VIT, ENE, CMD ou quaisquer atributos j├í presentes nos documentos/c├│digo. Registre o mapeamento ou a substitui├º├úo em uma decis├úo documentada. N├úo mantenha dois sistemas de atributos paralelos. Defina f├│rmulas, limites, efeitos por classe e testes para provar que cada atributo altera o resultado esperado.

---

## 6. Mapas e teleporte ÔÇö tecla M

A lista de mapas deve apresentar os mapas dispon├¡veis, seus requisitos e o custo de teleporte em **Coins**.

Cada mapa deve possuir dados consistentes, incluindo, conforme necess├írio:

- Identificador est├ível, nome e descri├º├úo.
- Requisito de n├¡vel e outros requisitos de acesso.
- Custo de teleporte em Coins.
- Ponto de chegada v├ílido.
- Regras de acesso, inst├óncia e/ou regi├úo.
- Monstros, NPCs, recursos e conte├║do associados.
- Estado de disponibilidade.

O teleporte deve validar no servidor o mapa de destino, n├¡vel, custo, saldo, estado do personagem e regras de acesso. O custo deve ser cobrado de forma at├┤mica: n├úo cobrar sem teletransportar e n├úo teletransportar sem cobrar quando o custo for obrigat├│rio. Mostrar claramente por que uma viagem foi recusada.

O cat├ílogo n├úo pode ser preenchido com mapas falsos que n├úo existam no mundo. Se o mapa ainda n├úo estiver implementado, identificar seu estado como planejado/bloqueado e impedir que seja apresentado como destino funcional.

---

## 7. Tela de carregamento, login e cria├º├úo de personagens

### 7.1 Tela de carregamento

Criar uma tela de carregamento polida com arte original de fantasia sombria mostrando v├írias classes em combate contra monstros e entre si, dentro de um castelo em ru├¡nas e em chamas. Usar recursos licenciados adequadamente ou assets originais; n├úo alegar que uma imagem ├® uma renderiza├º├úo real do jogo se for apenas arte conceitual.

A tela deve mostrar progresso ou estado real de carregamento sempre que tecnicamente poss├¡vel, com tratamento de falha, timeout e possibilidade de tentar novamente. Evitar uma barra de progresso fict├¡cia que avance sem rela├º├úo com o processo.

### 7.2 Login e contas

Ap├│s a etapa inicial de carregamento, exibir a tela de login com campos apropriados, valida├º├úo e mensagens claras. Implementar:

- Cadastro e autentica├º├úo de contas, conforme a arquitetura aprovada.
- Senhas armazenadas com hash apropriado e salt por biblioteca reconhecida; nunca em texto puro.
- Valida├º├úo de entrada, limites de tentativas e prote├º├úo b├ísica contra abuso.
- Sess├Áes/tokens tratados de forma segura, com expira├º├úo e revoga├º├úo conforme a arquitetura.
- Mensagens de erro que n├úo revelem informa├º├Áes sens├¡veis.
- Persist├¬ncia no servidor e tratamento de indisponibilidade de rede.
- Fluxo de logout e reconex├úo consistente.

N├úo adicionar senhas padr├úo ou credenciais reais ao reposit├│rio. Recursos como recupera├º├úo de senha, verifica├º├úo de e-mail e autentica├º├úo adicional devem ser definidos no roadmap ou numa decis├úo expl├¡cita se n├úo houver provedor/infraestrutura dispon├¡vel.

### 7.3 Lobby de cria├º├úo de personagem

Depois do login, permitir criar personagem em um lobby visual. O jogador deve poder:

- Escolher entre as classes efetivamente implementadas.
- Visualizar imagem/modelo da classe e uma breve descri├º├úo.
- Consultar atributos iniciais, estilo de combate e papel esperado.
- Informar um nome e receber valida├º├úo de formato, tamanho, caracteres e unicidade.
- Confirmar a cria├º├úo e receber resposta do servidor.
- Ver mensagens ├║teis quando o nome estiver ocupado ou a cria├º├úo for recusada.

A cria├º├úo deve persistir e n├úo gerar personagens duplicados por cliques repetidos ou falhas de rede. As classes exibidas precisam corresponder a classes realmente configuradas no conte├║do.

### 7.4 Sele├º├úo de personagens

Depois de criar um personagem, apresentar a tela de sele├º├úo com os personagens da conta organizados lado a lado, com modelo/imagem, nome, classe, n├¡vel e demais informa├º├Áes relevantes. Permitir selecionar e entrar no mundo com o personagem escolhido. Respeitar limites de personagens por conta se esse limite for definido.

N├úo usar uma lista est├ítica de personagens de demonstra├º├úo como substituto da consulta ao servidor.

---

## 8. Sistemas centrais de MMORPG

Todos os sistemas abaixo devem ser integrados entre si e evoluir conforme as fases do roadmap. N├úo ├® necess├írio implementar tudo na primeira entrega.

### Mundo e movimenta├º├úo

- Mapas 3D naveg├íveis, colis├Áes, limites, pontos de entrada e sa├¡da.
- Navega├º├úo por clique esquerdo, com destino v├ílido e feedback visual.
- Spawn de jogadores, monstros e NPCs baseado em conte├║do.
- Sincroniza├º├úo entre jogadores conectados e tratamento de desconex├Áes.
- Transi├º├úo de mapas e restaura├º├úo de estado ao reconectar.

### Classes e progress├úo

- Classes distintas com identidade visual, estilo de combate e habilidades.
- N├¡vel, experi├¬ncia, recompensas e curva de progress├úo configur├íveis.
- Pontos de atributo com valida├º├úo autoritativa.
- F├│rmulas de estat├¡sticas e crescimento documentadas e testadas.
- Balanceamento orientado por dados, sem n├║meros cr├¡ticos espalhados por scripts.

### Combate e habilidades

- Alvos, alcance, cooldowns, anima├º├Áes, feedback de acerto e dano.
- Ataques b├ísicos acionados por clique direito.
- Habilidades ativas/passivas, custos, requisitos, cooldowns e efeitos.
- Estados como morte, atordoamento, buffs/debuffs e invulnerabilidade apenas quando definidos e implementados.
- C├ílculo autoritativo no servidor, com valida├º├úo de dist├óncia, tempo e estado.
- F├│rmulas determin├¡sticas e testes para dano, mitiga├º├úo, cr├¡tico, recursos e efeitos.
- Feedback visual e sonoro sem substituir a valida├º├úo real.

### Monstros, NPCs e loot

- Dados de monstros com atributos, comportamento, ataques, recompensas e locais de spawn.
- IA b├ísica funcional: detectar, perseguir, atacar, retornar quando apropriado e morrer.
- NPCs com intera├º├úo contextual e servi├ºos implementados.
- Drops calculados no servidor, com tabelas de probabilidade configur├íveis.
- Experi├¬ncia e recompensas concedidas uma ├║nica vez e persistidas corretamente.
- Preven├º├úo de duplica├º├úo de loot e explora├º├úo de recompensas.

### Itens, equipamento e economia

- Cat├ílogo de itens com IDs est├íveis, tipo, raridade, ├¡cone, descri├º├úo e propriedades.
- Equipamentos com slots e requisitos compat├¡veis com classes/regras.
- Invent├írio, loot, consumo, lojas e moedas com opera├º├Áes validadas no servidor.
- Comprar, vender e usar itens com tratamento at├┤mico e feedback claro.
- Persist├¬ncia e prote├º├úo contra duplica├º├úo, saldo negativo e altera├º├Áes forjadas.
- Refinamento e crafting apenas conforme as regras e os dados definidos na fase correspondente.

### Miss├Áes, NPCs e bosses

- Objetivos e condi├º├Áes definidos em dados.
- Progresso rastre├ível e persistente.
- Recompensas concedidas uma ├║nica vez, com valida├º├úo no servidor.
- NPCs e bosses com comportamentos e recompensas pr├│prios.
- Regras de reset, repeti├º├úo e pr├®-requisitos documentadas.

### Sistemas sociais

Conforme as fases, construir chat, grupo/party, guildas, troca entre jogadores e demais recursos sociais definidos no roadmap. Validar permiss├Áes, participantes, estados e opera├º├Áes econ├┤micas no servidor. Trocas devem ser seguras contra altera├º├Áes de ├║ltima hora, desconex├Áes e duplica├º├úo de itens.

### PvP, PK, rankings e eventos

- PvP com regras expl├¡citas de alvo, dano, zonas e estados.
- PK e penalidades apenas quando formalmente definidos.
- Rankings calculados a partir de dados confi├íveis.
- Eventos e cerco com regras, participantes, dura├º├úo e recompensas configur├íveis.
- N├úo criar sistemas meramente decorativos nem expor comandos administrativos inseguros.

---

## 9. Multiplayer, banco e seguran├ºa

O servidor deve ser a autoridade para estado persistente e regras que possam afetar justi├ºa, progress├úo ou economia. O cliente envia inten├º├Áes; o servidor valida e responde com resultados.

### Persist├¬ncia

Persistir, conforme a arquitetura e as fases:

- Contas e credenciais protegidas.
- Personagens, classe, n├¡vel, experi├¬ncia e atributos.
- Localiza├º├úo e estado necess├írio ├á reconex├úo.
- Invent├írio, equipamentos, moedas e progresso.
- Miss├Áes, configura├º├Áes e dados sociais relevantes.

Definir migra├º├Áes versionadas, integridade referencial, transa├º├Áes para opera├º├Áes cr├¡ticas, backups e procedimentos de recupera├º├úo apropriados ao est├ígio do projeto. Testar rein├¡cio do servidor e reconex├úo para verificar que os dados permanecem ├¡ntegros.

### Seguran├ºa e confiabilidade

- Validar toda entrada recebida pela rede.
- Impor limites de frequ├¬ncia e tamanho das mensagens.
- Tratar desconex├Áes, timeouts, pacotes inv├ílidos e falhas de banco.
- Evitar confiar no rel├│gio, no dano ou nos saldos calculados pelo cliente.
- Prevenir repeti├º├úo indevida de requisi├º├Áes cr├¡ticas, duplica├º├úo de recompensas e transa├º├Áes parciais.
- N├úo registrar senhas, tokens ou dados secretos.
- N├úo abrir portas ou publicar servidores/credenciais sem autoriza├º├úo e documenta├º├úo.
- Testar abuso b├ísico, falhas de rede e opera├º├Áes concorrentes antes do lan├ºamento.

---

## 10. Arte 3D, anima├º├úo e ├íudio

A qualidade visual deve evoluir em conjunto com o gameplay, sem bloquear a entrega de uma base funcional.

- Personagens e classes visualmente distingu├¡veis.
- Monstros e NPCs com silhuetas e anima├º├Áes adequadas.
- Anima├º├Áes de idle, movimento, ataque, habilidade, dano e morte, conforme o conte├║do dispon├¡vel.
- Cen├írios com ilumina├º├úo, materiais, colis├Áes e navega├º├úo utiliz├íveis.
- Efeitos de impacto, habilidades, teleporte e loot, com legibilidade visual.
- ├üudio para interface, ambiente, combate e a├º├Áes importantes.
- Interface consistente com o mundo e o tema de fantasia sombria.
- Otimiza├º├úo de pol├¡gonos, texturas, materiais, efeitos e carregamento conforme as medi├º├Áes.

Use assets pr├│prios, gerados para o projeto ou com licen├ºas compat├¡veis. Registre origem e licen├ºa de assets externos. N├úo afirme que existem modelos, anima├º├Áes, efeitos ou sons que n├úo estejam realmente presentes e integrados. Se algum recurso art├¡stico avan├ºado n├úo puder ser produzido na fase atual, use uma alternativa provis├│ria claramente identificada, mantenha o jogo execut├ível e registre a pend├¬ncia.

---

## 11. Roadmap obrigat├│rio ÔÇö Fases 0 a 14

O `CHECKPOINT.md` existente define a sequ├¬ncia de 15 fases abaixo. Preserve seus nomes e sua ordem; detalhe tarefas conforme a auditoria do c├│digo e dos demais documentos. A lista a seguir descreve os resultados esperados e n├úo autoriza pular fases.

### Fase 0 ÔÇö Prepara├º├úo

**Objetivo:** estabelecer uma base t├®cnica reproduz├¡vel.

- Auditar reposit├│rio, documentos, ambiente e ferramentas instaladas.
- Confirmar a stack proposta em `02_ARQUITETURA.md` e registrar ADR-001.
- Definir estrutura de diret├│rios e conven├º├Áes de c├│digo.
- Preparar um processo de build/teste reproduz├¡vel.
- Criar um primeiro teste execut├ível do servidor e do cliente.
- Registrar vers├Áes, comandos de build, testes e execu├º├úo.
- Atualizar `CHECKPOINT.md` com fatos verificados.

**Aceite:** decis├úo arquitetural registrada; estrutura e scripts funcionam; cliente e servidor t├¬m um teste inicial reproduz├¡vel; documenta├º├úo indica como executar.

### Fase 1 ÔÇö Funda├º├úo do servidor

**Objetivo:** estabelecer o n├║cleo confi├ível do servidor.

- Inicializar servidor e biblioteca compartilhada.
- Implementar configura├º├úo, logging, tratamento de erros e ciclo principal.
- Definir protocolo inicial, vers├Áes e valida├º├úo de mensagens.
- Criar acesso ao banco, esquema inicial e migra├º├Áes versionadas.
- Estabelecer contas/sess├Áes na medida necess├íria para as fases seguintes.
- Implementar testes unit├írios e de integra├º├úo b├ísicos.

**Aceite:** servidor inicia de forma reproduz├¡vel; banco e migra├º├Áes funcionam; testes b├ísicos passam; entradas inv├ílidas s├úo tratadas.

**Incremento atual (parcial, ainda n├úo aceito por falta de build/runtime):** PostgreSQL/Npgsql, migration versionada, registro/login, bearer, ownership de personagens, audit log, lista real sem classe inventada e sess├úo Coast com movimento autoritativo foram acrescentados. `npm run test:phase1` inclui DB descart├ível, sess├úo, ownership, spawn, velocidade, colis├úo/navmesh, desconex├úo/reconex├úo e persist├¬ncia; requer SDK .NET 10 e Docker e permanece bloqueado at├® execut├í-lo. Veja `server/README.md` e `CHECKPOINT.md`.

### Fase 2 ÔÇö Cliente base e login

**Objetivo:** conectar o cliente ao servidor e habilitar acesso de conta.

- Inicializar o cliente e sua estrutura de cenas/telas.
- Implementar tela de carregamento e login.
- Conectar autentica├º├úo ao servidor.
- Tratar carregamento, erro, desconex├úo e nova tentativa.
- Criar os primeiros testes do fluxo cliente-servidor.

**Aceite:** cliente conecta-se ao servidor e autentica uma conta de teste por fluxo real; erros s├úo apresentados de forma compreens├¡vel; credenciais n├úo s├úo expostas.

### Fase 3 ÔÇö Mundo e movimento

**Objetivo:** primeira fatia jog├ível com entrada real em Coast e movimento server-authoritative.

- Abrir Coast via uma sess├úo criada/retomada pelo servidor ap├│s ownership check.
- Instanciar personagem selecionado no spawn autorizado; validar movimento por WASD/setas, ticks, velocidade e navega├º├úo no servidor.
- Tratar colis├Áes/bordas, orienta├º├úo, c├ómera, Idle/Walking, desconex├úo/reconex├úo, encerramento e retorno ao lobby.
- Manter multiplayer, sincroniza├º├úo entre pares e movimento por clique como trabalho futuro fora desta fatia; n├úo chamar o jogador local de multiplayer.

**Aceite desta fatia:** login ÔåÆ personagens da API ÔåÆ sele├º├úo ÔåÆ entrada autorizada ÔåÆ Coast ÔåÆ spawn ÔåÆ caminhar com estado/velocidade validados pelo servidor e parada em limites naveg├íveis. Build Godot/.NET, teste PostgreSQL e inspe├º├úo visual/f├¡sica continuam obrigat├│rios; testes est├íticos n├úo aceitam a fase.

### Fase 4 ÔÇö Combate, monstros e habilidades

**Objetivo:** criar o primeiro ciclo real de combate.

- Implementar sele├º├úo de alvo e ataque b├ísico por clique direito.
- Criar monstros com spawn e IA b├ísica.
- Implementar vida, dano, morte e recompensas iniciais.
- Adicionar estrutura de habilidades, cooldowns e custos.
- Validar alcance, estado e frequ├¬ncia de a├º├Áes no servidor.
- Criar testes de f├│rmulas e fluxo de combate.

**Aceite:** o jogador ataca monstros reais; o servidor calcula o resultado; estados de vida/morte e recompensas funcionam; testes cobrem casos v├ílidos e inv├ílidos.

### Fase 5 ÔÇö Itens, invent├írio, drops e lojas

**Objetivo:** criar o ciclo inicial de itens e economia.

- Criar cat├ílogo de itens e valida├º├úo de conte├║do.
- Implementar invent├írio e interface da tecla I.
- Implementar drops, coleta, equipamentos e itens consum├¡veis.
- Implementar lojas e transa├º├Áes iniciais conforme o conte├║do definido.
- Persistir itens e saldos.
- Testar opera├º├Áes concorrentes e preven├º├úo de duplica├º├úo.

**Aceite:** itens podem ser obtidos e manipulados por fluxo real; invent├írio persiste; compras, drops e equipamentos s├úo validados pelo servidor.

### Fase 6 ÔÇö Progress├úo e classes

**Objetivo:** estabelecer personagens jog├íveis e progress├úo.

- Implementar classes efetivas, cria├º├úo e sele├º├úo de personagem.
- Implementar n├¡vel, experi├¬ncia, curva de progress├úo e pontos.
- Implementar painel P com estado real.
- Definir e documentar F├║ria, Defesa, Mana, Agilidade e Sabedoria, reconciliando atributos existentes.
- Implementar f├│rmulas, efeitos por classe e valida├º├úo de distribui├º├úo.
- Persistir o progresso.

**Aceite:** cria├º├úo/sele├º├úo funciona com dados persistidos; atributos alteram gameplay; progress├úo e painel refletem o estado real; f├│rmulas t├¬m testes.

### Fase 7 ÔÇö Social

**Objetivo:** habilitar intera├º├úo entre jogadores.

- Implementar os recursos sociais definidos na documenta├º├úo, incluindo chat e grupo/party conforme o escopo registrado.
- Definir estados, permiss├Áes, convites e tratamento de desconex├Áes.
- Implementar testes de autoriza├º├úo e sincroniza├º├úo.

**Aceite:** recursos sociais previstos funcionam entre clientes conectados e respeitam as permiss├Áes definidas.

### Fase 8 ÔÇö Refino, crafting e op├º├Áes

**Objetivo:** ampliar o ciclo de equipamentos e personaliza├º├úo.

- Implementar refino e crafting orientados a dados.
- Definir custos, materiais, resultados, probabilidades e falhas quando aplic├ível.
- Implementar op├º├Áes/configura├º├Áes do jogador que estiverem definidas no projeto.
- Validar e persistir as opera├º├Áes no servidor.

**Aceite:** receitas e regras s├úo validadas; materiais/custos s├úo debitados corretamente; n├úo h├í duplica├º├úo em falhas ou requisi├º├Áes repetidas.

### Fase 9 ÔÇö PvP, PK e ranking

**Objetivo:** implementar combate entre jogadores com regras expl├¡citas.

- Definir zonas e elegibilidade de PvP.
- Implementar dano entre jogadores e estados pertinentes.
- Implementar PK/penalidades apenas conforme regras aprovadas.
- Criar rankings baseados em dados confi├íveis.
- Testar permiss├Áes, exploits b├ísicos e resultados persistidos.

**Aceite:** PvP segue as regras documentadas; rankings n├úo podem ser manipulados pelo cliente; estados e penalidades s├úo consistentes.

### Fase 10 ÔÇö Quests, NPCs e bosses

**Objetivo:** ampliar o conte├║do de mundo.

- Implementar sistema de quests orientado a dados.
- Criar objetivos, pr├®-requisitos, progresso, conclus├úo e recompensas.
- Integrar NPCs interativos e servi├ºos previstos.
- Implementar bosses e mec├ónicas/recompensas definidas.
- Testar persist├¬ncia e concess├úo ├║nica de recompensas.

**Aceite:** quests podem ser iniciadas e conclu├¡das; progresso persiste; recompensas n├úo s├úo duplicadas; NPCs e bosses t├¬m fluxos funcionais.

### Fase 11 ÔÇö Eventos

**Objetivo:** implementar eventos do mundo e eventos instanciados previstos no projeto.

- Definir regras, calend├írio/ativa├º├úo, participantes e condi├º├Áes de conclus├úo.
- Implementar recompensas e encerramento seguro.
- Integrar cerco ou outros eventos de maior escala se estiverem definidos no escopo oficial.
- Testar concorr├¬ncia, desconex├Áes e estados de evento.

**Aceite:** eventos possuem ciclo de vida reproduz├¡vel e suas regras/recompensas s├úo validadas no servidor.

### Fase 12 ÔÇö Ferramentas de conte├║do

**Objetivo:** tornar o conte├║do sustent├ível e valid├ível.

- Implementar ou consolidar o validador de conte├║do.
- Validar refer├¬ncias, IDs, schemas, faixas num├®ricas e depend├¬ncias.
- Criar ferramenta de edi├º├úo de conte├║do somente conforme o escopo e as tecnologias aprovadas.
- Documentar o fluxo para adicionar mapas, itens, monstros, skills, NPCs e quests.
- Adicionar exemplos e testes de conte├║do inv├ílido.

**Aceite:** dados inv├ílidos s├úo detectados antes de serem usados; o fluxo de cria├º├úo de conte├║do ├® documentado e repet├¡vel.

### Fase 13 ÔÇö Performance, seguran├ºa e QA

**Objetivo:** preparar o jogo para testes ampliados e reduzir riscos.

- Executar testes de carga e simula├º├úo de jogadores conforme a capacidade do ambiente.
- Medir ticks do servidor, rede, uso de mem├│ria, banco e renderiza├º├úo.
- Corrigir gargalos demonstrados por medi├º├Áes.
- Revisar valida├º├úo de entrada, rate limits, autentica├º├úo, economia e permiss├Áes.
- Testar desconex├Áes, reconex├Áes, concorr├¬ncia, persist├¬ncia e recupera├º├úo.
- Corrigir falhas cr├¡ticas e documentar riscos restantes.

**Aceite:** testes e m├®tricas t├¬m resultados registrados; falhas cr├¡ticas conhecidas foram corrigidas ou est├úo claramente bloqueadas e documentadas; n├úo h├í alega├º├úo de capacidade sem evid├¬ncia.

### Fase 14 ÔÇö Polimento e lan├ºamento

**Objetivo:** produzir uma vers├úo utiliz├ível, documentada e reproduz├¡vel.

- Polir interface, anima├º├Áes, ├íudio, feedback e acessibilidade b├ísica.
- Verificar fluxos completos de conta, cria├º├úo/sele├º├úo, mundo, combate, invent├írio, progress├úo e demais sistemas previstos.
- Validar configura├º├úo de cliente e servidor para o ambiente de destino.
- Preparar instru├º├Áes de instala├º├úo, build, execu├º├úo, backup e solu├º├úo de problemas.
- Documentar limita├º├Áes conhecidas, depend├¬ncias e licen├ºas de assets.
- Produzir artefatos de build somente para plataformas realmente suportadas e testadas.

**Aceite:** instala├º├úo e execu├º├úo podem ser reproduzidas seguindo a documenta├º├úo; testes de regress├úo relevantes passam; os recursos ausentes e limita├º├Áes s├úo explicitamente listados.

---

## 12. Crit├®rios gerais de qualidade e conclus├úo

Uma funcionalidade ├® considerada conclu├¡da quando:

1. Est├í implementada e conectada ao fluxo real do jogo.
2. Tem valida├º├úo de entradas e regras pertinentes.
3. Atualiza o estado correto no cliente e/ou servidor.
4. Persiste dados quando necess├írio.
5. Tem feedback de sucesso, falha e estados inv├ílidos.
6. Possui testes adequados ou uma verifica├º├úo manual reproduz├¡vel quando automa├º├úo n├úo for vi├ível.
7. N├úo quebra funcionalidades existentes.
8. Sua documenta├º├úo est├í atualizada.

Uma fase ├® conclu├¡da apenas quando todos os seus crit├®rios de aceite foram verificados. Se um item estiver bloqueado por depend├¬ncia externa, marque a fase como **BLOQUEADA** ou **EM ANDAMENTO**, explique a evid├¬ncia e prossiga apenas com tarefas independentes seguras.

### Testes m├¡nimos a incorporar progressivamente

- Build limpo de cada componente relevante.
- Testes unit├írios de regras e f├│rmulas.
- Testes de integra├º├úo com banco e migra├º├Áes.
- Testes de protocolo e autentica├º├úo.
- Testes de fluxo cliente-servidor.
- Testes de persist├¬ncia ap├│s rein├¡cio.
- Testes com dois ou mais clientes para recursos multiplayer.
- Testes de entradas inv├ílidas, a├º├Áes n├úo autorizadas e opera├º├Áes repetidas.
- Testes de regress├úo dos controles por mouse e atalhos I/P/M.

Os comandos exatos devem ser descobertos, confirmados e registrados durante a Fase 0; n├úo inventar comandos nem declarar que foram executados se n├úo foram.

---

## 13. Checkpoint, hist├│rico e passagem de trabalho

O arquivo `CHECKPOINT.md` ├® a refer├¬ncia operacional para retomar o trabalho. Deve conter, no m├¡nimo:

- Fase atual e seu estado.
- ├Ültima tarefa conclu├¡da e evid├¬ncia.
- Data da atualiza├º├úo.
- Pr├│ximo passo exato, pequeno e execut├ível.
- Comandos reais de build, testes e execu├º├úo.
- Ambiente e vers├Áes verificadas.
- Resultados dos testes executados.
- Problemas abertos, bloqueios e suposi├º├Áes.
- Hist├│rico resumido append-only.

Ao finalizar uma sess├úo, deixe o reposit├│rio em estado compreens├¡vel para outro Agent. Evite terminar com uma tarefa vaga como ÔÇ£continuar desenvolvimentoÔÇØ. Especifique o arquivo/m├│dulo a trabalhar, o resultado esperado e como validar.

---

## 14. Formato obrigat├│rio do relat├│rio do Agent

Ao concluir uma tarefa ou interromper por bloqueio, apresente:

1. **Fase e objetivo:** fase atual e tarefa tratada.
2. **Implementado:** arquivos e comportamento efetivamente criados/alterados.
3. **Valida├º├úo:** comandos executados e resultados reais, incluindo falhas.
4. **Integra├º├úo:** como o recurso se conecta aos sistemas existentes.
5. **Documenta├º├úo:** arquivos atualizados.
6. **Pend├¬ncias/bloqueios:** o que ainda n├úo est├í pronto e por qu├¬.
7. **Pr├│ximo passo exato:** a pr├│xima tarefa de acordo com o roadmap e checkpoint.

N├úo confunda ÔÇ£planejadoÔÇØ, ÔÇ£iniciadoÔÇØ, ÔÇ£implementadoÔÇØ, ÔÇ£testadoÔÇØ e ÔÇ£conclu├¡doÔÇØ. N├úo apresente uma lista de inten├º├Áes como se fosse uma entrega pronta.

---

## 15. Primeira a├º├úo ao receber este README

O Agent deve come├ºar auditando o reposit├│rio e a documenta├º├úo, confirmando a fase atual registrada em `CHECKPOINT.md` e comparando-a com o estado real dos arquivos e testes. Consulte a situa├º├úo vigente em `CHECKPOINT.md`; instru├º├Áes antigas que afirmam Fase 0 s├úo hist├│ricas e n├úo substituem o estado mais recente.

A Fase 0 e a decis├úo ADR-001 s├úo hist├│ricas/conclu├¡das. Continue pelo pr├│ximo passo de gameplay registrado no `CHECKPOINT.md`; neste checkout a implementa├º├úo Coast est├í no c├│digo, mas .NET/Godot e teste Windows ainda s├úo bloqueios de valida├º├úo.

Depois, implemente, valide e documente o trabalho de forma incremental, seguindo o roadmap at├® a Fase 14. O objetivo ├® entregar um MMORPG real, e n├úo apenas um plano, uma demonstra├º├úo visual ou um conjunto de arquivos sem integra├º├úo.

---

## Estado e comandos deste checkout

O `CHECKPOINT.md` na raiz ├® a fonte operacional vigente. Textos de fases anteriores e o conte├║do hist├│rico em `rpg-pack/` descrevem roadmap/decis├Áes antigas; n├úo provam funcionalidade atual nem substituem os requisitos atuais de WASD/setas e entrada server-authoritative Coast.

O prot├│tipo Node/Canvas legado segue separado. Comandos locais:

```bash
npm ci
npm run check
npm run test:phase1       # requer SDK .NET e Docker/PostgreSQL descart├ível
npm run test:target       # requer Godot 4.7.2 Mono e .NET
```

`npm run check` executa Node/tests est├íticos/geometria; n├úo compila C# nem importa Godot. `/health/ready` requer migration 2. No Agent atual, `dotnet`, `godot`, `godot4`, Docker e PostgreSQL n├úo est├úo dispon├¡veis; portanto nenhuma build .NET/Godot, integra├º├úo execut├ível nem prova visual/f├¡sica foi conclu├¡da nesta etapa. A valida├º├úo Windows ├® somente em `D:\ProjetoRPG-Teste-Lobby`; n├úo utilizar `D:\AProjeto RPG Fixo`. Veja `docs/02_ARQUITETURA.md`, `docs/CONTRATO_FUTURO_SESSAO_GAMEPLAY.md`, `server/README.md` e `CHECKPOINT.md` para rotas/protocolo/testes/limites.
