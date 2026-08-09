# Классическая сборка

Приложение использует только системный .NET Framework, WinForms и стандартные библиотеки Windows. NuGet, WinUI, Windows App SDK и установленный современный .NET для запуска не нужны.

Обычная версия с минутами:

```powershell
.\build-classic.ps1
```

Тестовая версия с секундами:

```powershell
.\build-classic.ps1 -Test
```

Результат — один файл `artifacts/classic/TimeTracker.exe` или `artifacts/classic/TimeTracker-Test.exe`.

После запуска приложение находится в системном трее. Двойной щелчок по значку начинает рабочий интервал; контекстное меню содержит запуск, настройки и выход.
