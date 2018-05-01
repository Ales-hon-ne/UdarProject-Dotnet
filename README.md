# UdarProject-Dotnet

Программа реализует модель упруго-пластического тела, описанную
в [монографии](http://elibrary.ru/item.asp?id=19451006). Решаемая
задача — соударение двух тел.

## Требования:
* Windows 7+ x64
* [DotNet Core 2.0+](https://dotnet.github.io)

## Сборка и запуск:
```
cd [папка с проектом]
dotnet build -c Release
dotnet run [файл настроек расчёта]
```

## Примеры входных файлов

Файл материалов: *outmat.json*

Файл очереди расчёта: *cq.json*