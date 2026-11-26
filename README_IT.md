# BanMod

[🇬🇧 Read this README in English](README.md)

## Prima di usare BanMod: scegli la modalità corretta

BanMod offre due modalità. Usa quella adatta alle funzioni effettivamente attive:

- **Modded +25**: per modifiche al gameplay, ruoli personalizzati, funzioni host che cambiano il comportamento del gioco o qualsiasi funzione che possa influire sull'esperienza di un altro giocatore. Segui la [Among Us Mod Policy](https://www.innersloth.com/among-us-mod-policy/) e i requisiti tecnici correnti di Innersloth.
- **Vanilla**: destinata esclusivamente a funzionalità anti-cheat compatibili e modifiche visive locali che non cambiano il gameplay o l'esperienza di altri giocatori. “Vanilla” è il nome della modalità BanMod e non significa che il client non sia modificato.

In caso di dubbio, considera la lobby come moddata.

In **Modded +25**, i comandi restano invariati. Per nascondere agli altri giocatori il testo del comando, sostituisci `/` con `/cmd`: ad esempio, `/bm blu` diventa `/cmd bm blu`.

> [!CAUTION]
> BanMod include controlli anti-cheat e di compatibilità. Mod o componenti sconosciuti/incompatibili possono causare la disattivazione di BanMod o degli Extra Services. Se utilizzi un'altra mod legittima, contatta l'amministratore per una verifica di compatibilità.

---

Moderazione delle lobby, protezione anti-abuso, controlli host, ruoli personalizzati e modalità configurabili per Among Us.

[Sito](https://banmod.online/) · [Istruzioni](https://banmod.online/instructions) · [Download](https://banmod.online/downloads) · [Privacy, Termini e Cookie](https://banmod.online/policies)

## Descrizione

BanMod è una mod per Among Us su Windows basata su BepInEx IL2CPP.

Il core pubblico GPLv3 comprende strumenti di moderazione, protezioni anti-abuso, amministrazione host, opzioni di gioco, ruoli personalizzati e interfacce di supporto.

Il repository pubblico contiene il core. Alcune funzioni opzionali distribuite tramite i servizi BanMod, storicamente chiamate “Premium” e ora descritte come **Extra Services**, sono separate dal core pubblico e non sono necessarie per compilarlo o utilizzarlo.

## Funzioni principali

- **Moderazione:** ban e blocchi persistenti, liste di giocatori sospetti, filtri per nomi e parole, protezione spam, gestione AFK e amministrazione giocatori.
- **Controlli host:** avvio automatico, regole di meeting e votazione, task, sabotaggi, porte, mappe, messaggi lobby, riepiloghi e azioni configurabili.
- **Ruoli e modalità:** ruoli personalizzati o modificati, preset, configurazione ruoli, miglioramenti Hide and Seek e modalità di test.
- **Strumenti client e grafici:** tasti configurabili, zoom negli stati consentiti, decorazioni, tema scuro, interfacce personalizzate, menu outfit/skin e opzioni locali.
- **Anti-cheat e servizi collegati:** verifica opzionale, report, messaggi server, sistemi anti-abuso e servizi lobby.

Le funzioni host, debug o di test devono essere usate in ambienti appropriati e rispettando gli altri giocatori della lobby.

## Immagini

Le immagini già presenti nel repository in `docs/images/` fanno parte della documentazione e devono essere mantenute senza modifiche.

Le skin personalizzate BanMod sono contenuti proprietari separati e non sono distribuite sotto GPLv3. Consulta [LICENSES.md](LICENSES.md).

## Requisiti

- Una copia legittima di Among Us per Windows PC.
- Una versione del gioco supportata dalla release BanMod corrente.
- Il pacchetto corretto per Steam o Epic Games.
- Permesso di estrarre file nella cartella che contiene `Among Us.exe`.

Gli aggiornamenti di Among Us possono rompere la compatibilità. Controlla sempre la release più recente prima di installare BanMod o segnalare un problema.

## Installazione

1. Scarica il pacchetto corrente dalla [pagina ufficiale di download](https://banmod.online/downloads).
2. Seleziona la versione Steam o Epic Games.
3. Apri la cartella che contiene `Among Us.exe`.
4. Estrai **tutti** i file dello ZIP BanMod in quella cartella.
5. Verifica che `Among Us.exe` e la cartella `BepInEx` siano allo stesso livello.
6. Avvia Among Us. Dopo il caricamento di BepInEx, BanMod dovrebbe comparire nel menu principale.

**Steam:** Libreria → clic destro su Among Us → Gestisci → Sfoglia file locali.  
**Epic Games:** Libreria → menu con tre puntini accanto ad Among Us → Gestisci → icona cartella.

### Aggiornamento e disinstallazione

Quando indicato nelle note di rilascio, salva la cartella dati/configurazione BanMod che vuoi conservare.

Rimuovi DLL BanMod obsolete o duplicate da `BepInEx/plugins` e non mischiare file provenienti da release diverse.

Per disinstallare, salva eventuali preset o configurazioni da conservare, poi usa la verifica dei file della piattaforma:

- **Steam:** Proprietà → File installati → Verifica integrità dei file di gioco.
- **Epic Games:** Gestisci → Verifica.

## Controlli predefiniti

- `Delete`: apre il menu principale BanMod.
- `F10`: apre il menu di configurazione dei tasti.

Tasti e menu possono cambiare in base alla release o ai permessi host. Consulta la guida in-game e le [istruzioni ufficiali](https://banmod.online/instructions).

## Extra Services

Alcune funzioni opzionali vengono distribuite separatamente tramite i servizi ufficiali BanMod.

Non sono necessarie per compilare o usare il core GPLv3. Disponibilità, requisiti di compatibilità, regole dei servizi, informazioni privacy e termini applicabili sono mantenuti sul sito ufficiale:

**https://banmod.online/policies**

Alcuni componenti separati possono inoltre avere condizioni di licenza differenti. Consulta [LICENSES.md](LICENSES.md).

## Policy e uso responsabile

Usa BanMod responsabilmente. Non utilizzarlo per cheat, molestie, interferenze dannose, abuso delle API, aggiramento delle protezioni, report falsi o vantaggi sleali.

Usa la modalità BanMod corretta per le funzioni attive e rispetta il consenso e l'esperienza degli altri giocatori.

Le regole complete e aggiornate relative ai servizi BanMod, privacy, trattamento dei dati, sicurezza, report, Community/Chat, Extra Services, Termini di utilizzo e cookie sono mantenute qui:

**[BanMod — Privacy, Termini e Cookie](https://banmod.online/policies)**

Per i requisiti specifici di Among Us consulta sempre la versione corrente della:

**[Among Us Mod Policy — Innersloth](https://www.innersloth.com/among-us-mod-policy/)**

Nel repository è disponibile anche un breve riepilogo in [POLICY_IT.md](POLICY_IT.md).

## Fork e build modificate

Il core pubblico coperto da GPLv3 può essere studiato, modificato e redistribuito secondo i termini della GPLv3.

Se distribuisci una build modificata:

- conserva licenza, avvisi e attribuzioni applicabili;
- indica chiaramente che si tratta di una versione non ufficiale e modificata;
- fornisci il codice sorgente corrispondente quando richiesto dalla GPLv3;
- non includere componenti server privati, credenziali, token, dati personali, asset proprietari BanMod o file del gioco senza autorizzazione;
- non lasciare intendere un'approvazione da parte di BanMod, GianniBart, Among Us o Innersloth.

Consulta [LICENSE](LICENSE) e [LICENSES.md](LICENSES.md).

## Compilazione dal sorgente

Il progetto utilizza .NET 6 e pacchetti BepInEx IL2CPP:

```bash
git clone https://github.com/GiannBart/BanMod.git
cd BanMod
dotnet restore
dotnet build -c Release
```

Prima della compilazione controlla `BanMod.csproj`: rimuovi percorsi Windows specifici dell'ambiente di sviluppo, configura assembly e metadata IL2CPP usando la tua installazione legittima del gioco e rimuovi eventuali target post-build locali.

Non pubblicare segreti, credenziali, configurazioni locali, binari di Among Us, `Among Us_Data`, `GameAssembly.dll` o altri file del gioco.

La DLL viene normalmente generata in:

```text
bin/Release/net6.0/
```

## Contributi e crediti

Issue e pull request per il core GPL sono benvenute se rispettano persone, legge applicabile, licenze e obiettivi del progetto.

Non inviare componenti proprietari, codice del gioco ottenuto illecitamente, endpoint segreti, credenziali o dati personali.

BanMod contiene lavoro originale e parti ispirate o derivate da progetti open source. Conserva gli avvisi presenti nei file sorgente e in `Resources/Credits and License.txt`.

Principali progetti accreditati:

- Town of Host
- Town of Host Enhanced
- EndlessHostRoles
- AmongUsRevamped
- MalumMenu
- TheOtherRoles / TheOtherHats
- BetterAmongUs
- GameLogger
- componenti e contributori NLayer, con licenza MIT dove indicato
- Spam bot database by AUGL (https://augl.net)

I crediti non implicano affiliazione o approvazione.

## Licenze

- Core pubblico BanMod: GNU GPLv3, salvo file con un diverso avviso compatibile.
- Codice e librerie di terze parti: licenze e avvisi originali.
- Componenti separati distribuiti dal server: consulta [LICENSES.md](LICENSES.md).
- Skin personalizzate BanMod: contenuto proprietario separato.
- Nomi, personaggi, loghi e materiali relativi ad Among Us appartengono a Innersloth LLC e/o ai rispettivi licenzianti.

Consulta [LICENSE](LICENSE) e [LICENSES.md](LICENSES.md).

## Avviso Innersloth

BanMod è una mod non ufficiale realizzata dalla community e non è affiliata a Innersloth.

Testo ufficiale:

> This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

Consulta sempre la [Among Us Mod Policy](https://www.innersloth.com/among-us-mod-policy/) corrente prima dell'utilizzo.

## Supporto

- Sito: https://banmod.online/
- Email: `banmod.giannibart@gmail.com`
- Discord: `GianniBart`
- Telegram: `@GianniBart`
- Bug del core GPL pubblico: GitHub Issues

Quando segnali un problema, includi versione BanMod, versione Among Us, piattaforma, passaggi per riprodurre il problema e log ripuliti. Non pubblicare token, friend code, identificativi giocatore, indirizzi email, messaggi privati o altri dati personali.
