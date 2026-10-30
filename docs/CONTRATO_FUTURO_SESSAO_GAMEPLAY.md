# Contrato futuro de sessão de gameplay, spawn e estado inicial

**Estado:** proposta de requisitos; não implementado e não publicado como contrato de API.
**Escopo:** separar o futuro ingresso autoritativo no mundo do lobby de conta/personagens.

## Estado atual

O cliente implementa cadastro, login, listagem/criação de personagens e logout por rotas existentes de conta. A seleção no lobby é apenas seleção de interface e prévia 3D genérica. O servidor ainda não expõe uma operação de ingresso em mundo, não cria uma sessão de gameplay, não escolhe spawn e não retorna snapshot inicial de mundo. O botão “Entrar no mundo” deve permanecer desabilitado.

Nenhuma rota, corpo, código HTTP, identificador de mundo, coordenada, atributo, classe ou regra de spawn abaixo deve ser interpretada como implementação existente. Os detalhes de transporte e o schema só devem ser definidos após uma decisão de arquitetura e versionamento do protocolo.

## Invariantes obrigatórios

1. **Identidade derivada da autenticação:** o servidor valida o Bearer e obtém nele a conta. O cliente não pode escolher nem enviar um `accountId` autoritativo.
2. **Ownership verificado no servidor:** o servidor confirma que o personagem solicitado pertence à conta autenticada. O cliente não concede acesso a partir de uma seleção local, GUID ou resposta previamente armazenada.
3. **Mundo e instância autoritativos:** a decisão de disponibilidade, região/instância e capacidade é do servidor. Não aceitar do cliente coordenada, mapa, estado, vida, recursos, inventário ou permissões como estado inicial confiável.
4. **Spawn validado pelo servidor:** selecionar um ponto configurado e válido para o mundo/instância, aplicar restrições de colisão e ocupação pertinentes e retornar o resultado efetivamente escolhido. Não inferir spawn a partir do lobby, da demo 3D ou de dados locais.
5. **Criação de sessão atômica e concorrência definida:** a associação jogador/personagem/mundo deve evitar sessões duplicadas ou estados parciais em chamadas simultâneas, retries e reconexões. Definir explicitamente idempotência, expiração e revogação antes de expor a operação.
6. **Snapshot inicial produzido pelo servidor:** após confirmar o ingresso, devolver apenas dados que o servidor realmente mantém e validar. Identificar versão/protocolo e revisão do estado necessários à sincronização futura. Não preencher classe, nível, atributos, posição, recursos ou aparência com valores fictícios.
7. **Transporte realtime autenticado:** estabelecer como a credencial/sessão de gameplay será vinculada ao canal realtime, como reconexão será autorizada e como mensagens de outro jogador/personagem serão recusadas. Nunca confiar no ID informado em mensagens do cliente.
8. **Falhas sem criação parcial:** sessão expirada/invalidada, personagem inexistente ou não pertencente, mundo indisponível, falta de spawn e conflito de sessão devem ter resultados documentados. Uma falha não pode deixar ingresso parcialmente criado.

## Requisitos para a futura especificação

Antes de implementar, registrar em ADR/API document:

- formato e versão da requisição/resposta, sem fixar aqui uma rota;
- comportamento para autenticação inválida e política de não revelar ownership;
- tratamento de personagem inexistente, duplicidade de ingresso e retries;
- seleção de mundo/instância, capacidade e disponibilidade;
- fonte, validação e persistência do spawn;
- campos exatos do snapshot inicial e origem autoritativa de cada campo;
- expiração/revogação, desconexão, reconexão e limpeza de sessão;
- relação entre resposta HTTP de ingresso e credencial do transporte realtime;
- garantias de atomicidade, idempotência e observabilidade sem registrar tokens ou segredos.

## Testes de aceitação antes de habilitar o botão

Implementar testes de integração contra o servidor e armazenamento descartáveis que provem:

- sessão válida e personagem pertencente: ingresso cria uma sessão única e retorna o estado inicial realmente persistido/configurado;
- sessão ausente, expirada ou revogada: recusa sem criar estado de gameplay;
- personagem de outra conta e personagem inexistente: recusa sem revelar dados de outro proprietário;
- mundo indisponível, capacidade excedida ou nenhum spawn válido: recusa sem sessão parcial;
- chamadas simultâneas, repetidas e reconexão: respeitam a política de idempotência e não duplicam jogador/personagem;
- identidade do canal realtime deriva da sessão validada, não de campos de identidade enviados pelo cliente;
- reinício/reconexão preserva somente o estado que o servidor reconhece como autoritativo.

Testes estáticos, renderização do lobby, inicialização do Bootstrap ou uma seleção local de personagem não satisfazem estes critérios.

## Critério para o cliente

Só habilitar o botão depois de a especificação e o endpoint autoritativo existirem e os testes de aceitação passarem. O cliente envia a identidade do personagem selecionado, apresenta sucesso apenas após confirmação do servidor e renderiza o snapshot recebido. Uma falha deve manter o usuário no lobby sem fabricar posição, spawn ou sessão. Movimento e mutações subsequentes também exigirão validação/autorização do servidor; o ingresso não os torna automaticamente seguros.


## Atualização de estado — 2026-10-30

O lobby agora mantém seleção visual account-backed por personagem; isso não altera os invariantes acima nem cria sessão de jogo. A importação Coast contém um marcador de spawn de cena em `(0.148617, 2.25, 51.4204)`, escolhido por análise estática porque cai sobre um triângulo da NavigationMesh original. **Esse marcador é somente dado de cena e não é spawn autorizado pelo servidor.** A API atual não publica mapa, world session, posição ou snapshot e o WebSocket ainda só oferece Hello/Ping/Bye anônimo.

O botão continua desabilitado. Antes de habilitá-lo, decidir e implementar versionadamente a transação de ingresso, validação da sessão bearer e ownership, mapa ativo no catálogo do servidor, escolha/revalidação do spawn, idempotência concorrente, armazenamento/expiração/reconexão/revogação e vínculo de identidade do canal realtime. Em seguida, implementar movimento por intenção com estado/velocidade/collision/navigation calculados pelo servidor, sincronização do snapshot e comportamento seguro após desconexão/restart. Não aceitar posição final enviada pelo cliente.

A prova de que o ponto está em um triângulo é estática e não prova que o mapa importe, que o ponto esteja no chão visual, que o ator caiba, nem que caminho/collision funcionem. Godot 4.7.2 Mono, .NET, DB integration e prova visual/física permanecem pendentes. Os critérios de aceitação anteriores seguem obrigatórios e nenhum route/body proposto neste arquivo é endpoint existente.
