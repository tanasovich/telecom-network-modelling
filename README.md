# Моделирование интерференционных помех

Моделирование интерференционных помех по исходным данным и вывод SNR показателей.
Расчет выполняется для _традиционных_ систем.

## Вввод

Передача входных данных выполняется с помощью файла `appsettings.xml` Также приложение
ожидает два файла с расширением `.data`:
- импульсные реакции
- спектральная маска

Каждый файл представляет собой список чисел, разделенных одним пробелом.

### Вывод

SNR показатели будут выведены в один файл с расширением `.data`
