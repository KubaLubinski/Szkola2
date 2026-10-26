# Szkoła – strona internetowa szkoły podstawowej

Strona internetowa szkoły z częścią publiczną dla odwiedzających i panelem do zarządzania treścią dla administratora. Zrobiłem ją jako projekt do portfolio. Wszystkie dane są zmyślone.

Adres działającej strony: https://jlubinskiszkola.runasp.net (po wybraniu zaloguj można utworzyć konto testowe)

##Konto testowe

- pozwala na utworzenie i edycje swojego wpisu na stronie,
- przeglądanie reszty zawartości strony, również tej dla administratora,
- strona jednak ma wbudowane zabezpieczenia, aby konto testowe nie mogło ingerować w jej działanie,
- dane dodawane przez konta testowe są regularnie usuwane,
- konta testowe mają określoną ważność, po jej upływie zostają usunięte.

## Zrzuty ekranu

<img width="1863" height="926" alt="glowna_jasny" src="https://github.com/user-attachments/assets/c8cfc1f5-a976-48fb-b7da-3fc44ccef874" />
<img width="1869" height="920" alt="glowna_ciemny" src="https://github.com/user-attachments/assets/8cad6272-22b2-41e3-997b-7bed32264b25" />
<img width="1860" height="919" alt="glowna_tresc" src="https://github.com/user-attachments/assets/184f6f41-a3b8-4421-8fcb-c8f1a2f835aa" />
<img width="1852" height="892" alt="aktualnosci_jasny" src="https://github.com/user-attachments/assets/5c303704-6c2f-468c-bb43-8fe11966836f" />
<img width="1860" height="917" alt="lista_dokumentow_ciemny" src="https://github.com/user-attachments/assets/045b5ca8-ed97-438c-b0e7-fc5559d4a450" />
<img width="1864" height="921" alt="zarzadzanie_wpisami_ciemny" src="https://github.com/user-attachments/assets/36045b29-e54c-45a0-866a-71db716b5b51" />
<img width="476" height="893" alt="glowna_mobilna" src="https://github.com/user-attachments/assets/c93daaa3-b43c-4341-9fb3-9f0a1b5ed4e6" />
<img width="475" height="906" alt="aktualnosci_mobilna" src="https://github.com/user-attachments/assets/4665410f-30dc-4925-87af-46044e2fa2c2" />


## Co potrafi strona

**Odwiedzający mogą:**
- utworzyć konto testowe,
- czytać aktualności podzielone na kategorie,
- zgłębiać historię szkoły,
- przeglądać kalendarium wydarzeń z minionych lat,
- pobierać dokumenty,
- dowiedzieć się więcej o projektach i programach prowadzonych w szkole.

**Administrator może:**
- zalogować się na własne konto,
- dodawać i edytować wpisy o aktualnościach z życia szkoły, wpisy w kalendarium, zdjęcia promocyjne na stronie głównej, wpisy o historii szkoły, kategorie wpisów, dokumenty i użytkowników,
- zobaczyć zadania wykonywane w tle (Handfire).

## Technologie

- **ASP.NET Core MVC (.NET 8)** – główny framework, strony pisane w Razor
- **Entity Framework Core + SQL Server** – baza danych
- **ASP.NET Core Identity** – logowanie, konta i role
- **Hangfire** – zadania w tle, np. automatyczne usuwanie kont demo
- **ImageSharp** – automatyczne zmniejszanie zdjęć i tworzenie miniatur
- **Serilog** – zapis logów do pliku
- **Bootstrap 5** – wygląd i układ na różnych ekranach
- **xUnit + GitHub Actions** – testy uruchamiane automatycznie przy każdym wrzuceniu zmian

## Bezpieczeństwo

- hasło musi mieć min. 8 znaków, min. 1 cyfra i 1 znak specjalny, a po 5 błędnych próbach konto blokuje się na 15 minut,
- logowanie ma limit prób, żeby nikt nie mógł zgadywać haseł w nieskończoność,
- tekst wpisany w edytorze jest czyszczony z niebezpiecznego kodu HTML,
- wysyłane pliki są sprawdzane (rodzaj pliku i rozmiar), a usuwanie plików nie pozwala wyjść poza dozwolony folder,
- w logach nie zapisuję adresów e-mail ani haseł,


## Jak uruchomić u siebie

Potrzebujesz: .NET 8 SDK oraz SQL Server lub LocalDB (jest w Visual Studio).

```bash
git clone https://github.com/KubaLubinski/Szkola2.git
cd Szkola2/Szkola2

dotnet ef database update
dotnet run
```

Pierwsze polecenie tworzy pustą bazę, a drugie uruchamia stronę. Przy pierwszym starcie program zakłada konto administratora, a jego hasło pokazuje **tylko w oknie konsoli**.

Jeśli chcesz użyć własnej bazy, ustaw adres połączenia tak:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=...;..."
```

## Schemat plików

```
Szkola2/
  Controllers/     obsługa żądań ze strony
  Models/          dane i modele
  Views/           wygląd stron (Razor)
  Data/            baza danych, migracje, dane startowe
  Infrastructure/  zdjęcia, pliki, zadania w tle, konta demo
  Areas/Identity/  logowanie i konta
  wwwroot/         CSS, JavaScript, zdjęcia
Szkola2.Tests/     testy
```
