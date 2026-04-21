# keycass_trompete

Teclado experimental com **5 teclas** (matriz direct pin no ATmega32u4) e **combos** QMK para obter o restante do alfabeto. O layout e os combos podem ser gerados a partir de `config/keymap.json` com o projeto .NET desta pasta.

**Mantenedor:** [Renato Cassino](https://github.com/renatocassino)  
**Bootloader:** Caterina (comum em Pro Micro / clones)  
**Bootmagic:** desligado no firmware — entrada no bootloader é por **reset físico** ou procedimento do Caterina (por exemplo duplo reset rápido, conforme a placa).

---

## Onde este projeto deve ficar

O QMK espera cada teclado **dentro** da árvore `qmk_firmware`, em `keyboards/<nome>/`.

Caminho esperado:

```text
qmk_firmware/
└── keyboards/
    └── keycass_trompete/    ← este repositório (ficheiros como keyboard.json, keymaps/, Makefile, …)
```

Ou seja: o clone/cópia deste repo deve resultar em algo equivalente a:

`/home/cassinodev/qmk_firmware/keyboards/keycass_trompete`

**Não** coloques só o `keymap.c` em outro lugar: o build usa `keyboard.json`, `keymaps/`, regras do QMK, etc., todos relativos à raiz do `qmk_firmware`.

### Como juntar o repo ao QMK

1. **Clonar o QMK oficial** (recomendado, com submódulos):

   ```bash
   cd ~
   git clone --recurse-submodules https://github.com/qmk/qmk_firmware.git
   ```

   (Se já tiveres `qmk_firmware` sem submódulos: `cd qmk_firmware && git submodule update --init --recursive`.)

2. **Colocar o keycass_trompete** em `qmk_firmware/keyboards/keycass_trompete`, por um destes modos:

   - **Clonar este repo** directamente para essa pasta (se o teu GitHub for a “fonte” do teclado):

     ```bash
     cd ~/qmk_firmware/keyboards
     git clone https://github.com/<teu-user>/keycass_trompete.git
     ```

   - **Ou** tens o projeto noutro sítio: **copia** ou **move** a pasta inteira para `keyboards/keycass_trompete`, ou cria um **symlink** para lá.

Até o `qmk compile -kb keycass_trompete` encontrar a pasta `keyboards/keycass_trompete` dentro do `qmk_firmware` que estás a usar, está correcto.

---

## Instalar o ambiente de build (QMK)

O QMK precisa de **ferramentas de compilação AVR** (para ATmega32u4), **Python**, e do **CLI `qmk`**. O guia oficial cobre todos os SO:

- [Ferramentas / ambiente](https://docs.qmk.fm/#/getting_started_build_tools)
- [Guia para principiantes](https://docs.qmk.fm/#/newbs)

### Linux (resumo)

1. Instala dependências de sistema conforme a tua distro (compilador AVR, avrdude, Python, etc.) — segue a documentação acima ou o script de utilitários do repo QMK, por exemplo:

   ```bash
   cd ~/qmk_firmware
   ./util/qmk_install.sh
   ```

   (O nome exacto do script pode variar com a versão do QMK; se não existir, usa só a doc “Build Tools”.)

2. **CLI Python `qmk`** (global ou em venv):

   - Opção global (exemplo): `pip install --user qmk` e garantir que o `PATH` inclui os scripts do pip.

   - **Opção neste teclado:** na pasta `keyboards/keycass_trompete` existe um alvo Make que cria um venv e instala o pacote `qmk`:

     ```bash
     cd ~/qmk_firmware/keyboards/keycass_trompete
     make install-qmk
     source ./python-env/bin/activate
     ```

     Depois de `activate`, o comando `qmk` fica disponível nesse terminal.

3. **Verificar:** na raiz do `qmk_firmware` (ou a partir da pasta do teclado, se o `qmk` estiver no `PATH`):

   ```bash
   qmk doctor
   ```

Corrige o que o `qmk doctor` apontar antes de compilar.

---

## Gerar o `keymap.c` a partir do JSON (opcional)

Se alterares `config/keymap.json` ou `keymaps/default/keymap.c.template`, regenera o C:

```bash
cd ~/qmk_firmware/keyboards/keycass_trompete
make build
```

Equivale a `dotnet run build` (processa o template e escreve `keymaps/default/keymap.c`).

Requisito: [.NET SDK](https://dotnet.microsoft.com/download) instalado.

---

## Compilar e gravar firmware

### Na raiz do `qmk_firmware` (Makefile “oficial” do QMK)

```bash
cd ~/qmk_firmware
make keycass_trompete:default
make keycass_trompete:default:flash
```

### Na pasta deste teclado (`keyboards/keycass_trompete/Makefile`)

Garante que `qmk` está no `PATH` (por exemplo com o venv activado).

| Comando | O que faz |
|--------|-----------|
| `make install-qmk` | Cria `./python-env` e instala o pacote Python `qmk` no venv. |
| `make build` | Corre `dotnet run build` (template → `keymap.c`). |
| `make compile` | `qmk compile -kb keycass_trompete -km default` |
| `make flash` | `qmk flash -kb keycass_trompete -km default` |
| `make update` | `make compile` seguido de `make flash` |

Equivalente directo com o CLI:

```bash
qmk compile -kb keycass_trompete -km default
qmk flash -kb keycass_trompete -km default
```

---

## Bootloader (Caterina)

Com **bootmagic** desligado, o reset por “segurar tecla ao ligar” **não** está activo neste keymap por defeito.

- Usa o **botão de reset** da placa (ou pads RST–GND), ou o método **duplo reset** típico de Pro Micro / Caterina para aparecer a porta série/USB para flash.

---

## Estudo de caso (ordenação de letras)

Literatura:  a e o s r i n d m u t c l p v g h q b f z j x k w y  
Wikipedia:   a e o i s r d n t c m u l p g b f v h q z j k x y w  
WhatsApp:    a e o i r s m u n t d c h l p v k q g b f z j x w y  

Proposta deste layout: a e o i s r d u m n c t l p b g f v q j z h k w x y
