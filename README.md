# E-Boekhouden Ledenexport

Desktop-app om snel een leden- of donateurlijst (export vanuit e-Boekhouden) uit te
draaien als PDF, CSV of ODS.

- Laadt een ledenlijst-export (.xlsx) uit de ledenadministratie — zowel een export met
  een echte Excel-tabel als een "kale" e-Boekhouden-export zonder tabelopmaak, waarbij
  de app zelf de header-rij (met "Lidnummer" en "Naam") opzoekt tussen eventuele
  titel-/datumregels erboven.
- Splitst de "Naam"-kolom automatisch in **Voornaam** en **Achternaam**, met correcte
  afhandeling van tussenvoegsels (bv. "Piet van Voorbeeld" → voornaam "Piet",
  achternaam "Voorbeeld, van") en gekoppelde achternamen (bv. "Anna Bakker-Jansen" blijft
  één achternaam, wordt niet op het streepje gesplitst).
- Toont alle rijen in een tabel, gesorteerd op achternaam en daarna voornaam (tussenvoegsels
  tellen niet mee voor de sortering, zoals gebruikelijk in het Nederlands: "van Voorbeeld"
  sorteert onder de V). Boven elke kolom staat een selectievakje dat bepaalt of die kolom
  wordt meegenomen in de export.
- Dropdown om te filteren: alle personen, alleen leden of alleen donateurs. De grens
  tussen lid en donateur is instelbaar via een invulveld ("Donateur vanaf lidnummer",
  standaard **10000**) — lidnummers eronder tellen als lid, erboven (of gelijk) als
  donateur.
- Export naar PDF, CSV (`;`-gescheiden) of ODS (OpenDocument Spreadsheet).
- Gebouwd met [Avalonia UI](https://avaloniaui.net/) (.NET), compileert naar een
  losstaande executable voor zowel Windows als Linux (getest op Ubuntu; werkt ook op
  Debian en CachyOS/Arch, mits de gebruikelijke grafische libraries aanwezig zijn).

## Vereisten

- [.NET SDK 10](https://dotnet.microsoft.com/download) (of nieuwer) om te bouwen.
- Op Linux: een grafische sessie (X11 of Wayland via XWayland) om de app te draaien.

## Project openen / bouwen

```bash
dotnet build
```

## Draaien tijdens ontwikkeling

```bash
dotnet run --project src/EBoekhouden.Ledenexport
```

## Gebruik

1. Klik op **Bestand openen…** en kies de ledenlijst-export (.xlsx).
2. Kies bij **Weergave** of je alle personen, alleen leden, of alleen donateurs wilt zien.
3. Pas zo nodig **Donateur vanaf lidnummer** aan (standaard 10000) — dit bepaalt
   direct de leden/donateurs-telling in de statusbalk en wat de filter hierboven laat zien.
4. Vink boven de gewenste kolommen het selectievakje aan/uit (**Alles aan**/**Alles uit**
   zetten in één keer alle kolommen aan of uit).
5. Klik op **CSV**, **PDF** of **ODS** en kies waar het bestand opgeslagen moet worden.

De export bevat alleen de aangevinkte kolommen, voor de rijen die bij de gekozen
weergave (alle/leden/donateurs) horen.

## Uitleveren als losstaande app (self-contained)

Deze commando's bouwen een enkel uitvoerbaar bestand dat geen aparte .NET-installatie
nodig heeft op de doelmachine.

**Windows (x64):**

```bash
dotnet publish src/EBoekhouden.Ledenexport -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64
```

Resultaat: `publish/win-x64/EBoekhoudenLedenexport.exe` (plus enkele native `.dll`'s ernaast
die niet in het single-file bestand passen — de hele map meenemen).

**Linux x64 (Debian, CachyOS, Ubuntu, …):**

```bash
dotnet publish src/EBoekhouden.Ledenexport -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish/linux-x64
```

Resultaat: `publish/linux-x64/EBoekhoudenLedenexport` (plus enkele native `.so`'s ernaast —
de hele map meenemen). Uitvoerbaar maken indien nodig met `chmod +x`.

**Linux ARM64 (bv. CachyOS op ARM):**

```bash
dotnet publish src/EBoekhouden.Ledenexport -c Release -r linux-arm64 --self-contained true -p:PublishSingleFile=true -o publish/linux-arm64
```

Op Linux zijn voor de GUI meestal wel de gebruikelijke desktop-libraries nodig
(fontconfig, X11 of Wayland/XWayland) — die staan standaard op zowel Debian als
CachyOS met een desktopomgeving.

## Snelste lokale build (bijv. rechtstreeks op CachyOS)

Voor eigen gebruik op de machine waar je ook bouwt, is lokaal bouwen sneller dan het
resultaat van een andere machine overzetten — en dat kan prima:

- **Tijdens ontwikkeling/eigen gebruik**, zonder publish: `dotnet run --project src/EBoekhouden.Ledenexport`
  (vereist de .NET SDK op die machine, geeft de snelste opstarttijd tijdens itereren).
- **Self-contained met ReadyToRun** voor de snelste opstarttijd van een uitgeleverde build:
  voeg `-p:PublishReadyToRun=true` toe zodat native code vooraf gecompileerd wordt
  (geen JIT-warmup meer bij het opstarten). Dit moet je bouwen **op** (of specifiek
  voor) hetzelfde platform als waar de app draait, dus bouwen op CachyOS zelf voor
  gebruik op CachyOS is precies de juiste aanpak:

  ```bash
  dotnet publish src/EBoekhouden.Ledenexport -c Release -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -o publish/linux-x64
  ```

## Projectstructuur

```
src/EBoekhouden.Ledenexport/
  Models/        Member-model (incl. lid/donateur-indeling) en de voornaam/achternaam-splitser
  Services/      Inlezen van de .xlsx-export (met of zonder Excel-tabel, header-detectie)
  Services/Export/  CSV-, PDF- en ODS-exporters
  ViewModels/    Kolomselectie, filter, donateurgrens en gridstate (MVVM)
  Views/         Het hoofdvenster (grid + toolbar)
data/            Lokale map voor je eigen ledenlijst-export (genegeerd door git, zie hieronder)
```

## Ledenlijst-bestand en privacy

De map `data/` staat in `.gitignore` en wordt nooit gecommit: een ledenlijst-export
bevat persoonsgegevens (naam, adres, e-mail, IBAN, …) en hoort niet in git terecht te
komen, ook niet in een private repository. Zet je eigen export daar lokaal neer (of
kies 'm via **Bestand openen…** ergens anders vandaan) — git negeert de map volledig.

## Licentie

Dit project is MIT-gelicentieerd, zie [LICENSE](LICENSE).

Let op: de PDF-export gebruikt [QuestPDF](https://www.questpdf.com/), dat niet onder
een standaard open-sourcelicentie valt maar onder de QuestPDF Community License —
gratis voor individuen en organisaties met een jaaromzet onder $1M, met een aparte
betaalde licentie daarboven. Dit raakt niet de MIT-licentie van deze app zelf, maar is
relevant als je deze app zelf bouwt/gebruikt binnen een grotere organisatie.
