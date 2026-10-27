# 10 — PVP EQUILIBRADO, PK E BALANCEAMENTO

## Problemas clássicos a evitar
Jogos desse estilo sofrem com: one-shot (morrer em 1 golpe), stun-lock sem fim, vantagem esmagadora de quem tem mais equipamento refinado, classes dominantes, PK sem consequência, "farm de iniciantes".

## Princípios
1. **PvP tem coeficientes próprios.** Todo dano/cura em PvP passa por `pvp.json` (multiplicadores por skill e por classe), independente do PvE.
2. **Teto de dano por golpe:** nenhum golpe causa mais que X% da vida máxima do alvo (padrão 35%, ajustável), evitando one-shot.
3. **Resistência a controle:** cada controle (stun, congelar, silêncio, lentidão) sofre redução de duração a cada aplicação em janela de tempo, com imunidade curta após o fim. Evita stun-lock.
4. **Compressão de gear em PvP:** bônus de refino acima de +N e opções excelentes têm retorno decrescente em dano/defesa PvP (curva suave), diminuindo a distância entre ricos e pobres sem anular progressão.
5. **Bônus de nível limitado:** diferença de nível acima de ~15 não aumenta mais o dano em PvP.
6. **Arenas normalizadas (opcional):** níveis e/ou equipamento padronizados, para PvP competitivo justo (modo "ranqueado").
7. **Matchmaking e ranking:** pontuação tipo Elo por modo (1x1, 3x3), temporadas, recompensas cosméticas ou de prestígio.
8. **Visibilidade e previsibilidade:** telegraph (aviso visual) em skills fortes, janelas de reação.
9. **Poções e recuperação em PvP:** recarga compartilhada e limite de uso.
10. **Telemetria:** registrar vencedor, classes, níveis, diferença de gear, duração, skills usadas. Painel de balance.

## Sistema de PK (matar jogadores neutros em áreas abertas)
- **Estados** (por contagem e tempo): Normal → Agressor (1 PK) → Assassino (3+ PKs) → Procurado (muitos).
- Cor do nome indica o estado. Penalidades crescentes: bloqueio de lojas e serviços, perda maior de XP ao morrer, chance de largar item não vinculado (nunca itens muito refinados ou vinculados), proibição de entrar em cidades protegidas por guardas (guardas atacam).
- **Autodefesa:** se A ataca B, B pode revidar por T segundos sem virar Agressor. Se A morrer, não ganha nada.
- **Redenção:** o estado decai com tempo online e caçando monstros; pode ser acelerado pagando Zen (custo cresce com o estado).
- **Defensor/Herói:** quem mata assassinos sem sofrer autodefesa ganha status temporário (menos perda de XP, bônus).
- **Proteção de iniciantes:** abaixo do nível N não pode ser alvo de PK, nem atacar.
- **Zonas:** cidade segura (sem PvP), campos (PvP com regras de PK), áreas de PvP livre (sem penalidade, com recompensas), arena (sem perdas), guerra de guildas (sem PK entre guildas em guerra).
- Registro de auditoria de todo PK para moderação.

## Processo de balanceamento
1. Simulação em massa (bots, mesma semente) para detectar outliers de classe.
2. Revisar números em `content/config/*.json`.
3. Playtest humano.
4. Patch de balance sempre por dados + changelog.
Meta inicial de equilíbrio: nenhuma classe acima de ~55–58% de vitórias em duelos de mesmo nível e equipamento comparável. Calibrar por medição.
