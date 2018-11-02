using System.ComponentModel;

namespace ReestrUslugOMS
{
    /// <summary>
    /// Перечисление видов источников данных
    /// </summary>
    public enum enDataSource:byte
    {
        [Description("")]                           Пусто = 0,
        [Description("Реестры-счетов")]             РеестрыСчетов = 1,
        [Description("Отчет")]                      Отчет = 2,
        [Description("План Отделения")]             ПланОтделения = 3,
        [Description("План Врача")]                 ПланВрача = 4
    }
    /// <summary>
    /// Перечисление видов данных
    /// </summary>
    public enum enDataType : byte
    {
        [Description("")]                           Пусто = 0,
        [Description("Код Врача")]                  КодВрача = 1,
        [Description("Код Отделения")]              КодОтделения = 2,
        [Description("Код Услуги")]                 КодУслуги = 3,
        [Description("Название Элемента")]          НазваниеЭлемента = 4,
        [Description("Номер Элемента")]             НомерЭлемента = 5,
    }
    /// <summary>
    /// Перечисление видов операций со значением данных
    /// </summary>
    public enum enOperation : byte
    {
        [Description("")]                           Пусто = 0,
        [Description("Умножение")]                  Умножение = 1,
        [Description("Деление")]                    Деление = 2,
        [Description("Умножение Выборочно")]        УмножениеВыборочно = 3,
        [Description("Не применять Выборочно")]     НеПрименятьВыборочно = 4,
    }
    /// <summary>
    /// Перечисление типов результатов
    /// </summary>
    public enum enResultType : byte
    {
        [Description("")]                           Пусто = 0,
        [Description("Услуг")]                      Услуг = 1,
        [Description("Количество услуг")]           КоличествоУслуг = 2,
        [Description("УЕТ")]                        УЕТ = 3,
        [Description("Услуг до 3 лет")]             УслугДо3Лет = 4,
        [Description("Услуг после 3 лет")]          УслугПосле3Лет = 5,
        [Description("Вложенные Элементы")]         ВложенныеЭлемены = 6,
        [Description("Проценты Делимое")]           ПроцентыДелимое = 7,
        [Description("Проценты Делитель")]          ПроцентыДелитель = 8,
        [Description("Диспансеризация Раз в 2 Года")]   ДиспансеризацияРазВ2Года = 9,
        [Description("Элементы Текущей Группы")]        ЭлементыТекущейГруппы = 10

    }
    /// <summary>
    /// Перечисление видов пациентов относительно места страхования
    /// </summary>
    public enum enInsuranceMode : byte
    {
        [Description("")]                           Пусто = 0,
        [Description("Все пациенты")]               ВсеПациенты = 1,
        [Description("Без иногородних")]            БезИногодних = 2,
        [Description("Только иногородние")]         ТолькоИногородние = 3
    }
    /// <summary>
    /// Перечисление видов услуг относительно статуса оплаты
    /// </summary>
    public enum enErrorMode : byte
    {
        [Description("")]                           Пусто = 0,
        [Description("Все услуги")]                 ВсеУслуги = 1,
        [Description("Без ошибок")]                 БезОшибок = 2,
        [Description("Только ошибки")]              ТолькоОшибки = 3
    }
    /// <summary>
    /// Перечисление видов отчета
    /// </summary>
    public enum enReportMode :byte
    {
        none = 0,        
        ПланВрача = 1,
        ПланОтделения = 2,
        Отчет = 3
    }
    /// <summary>
    /// Перечисление видов суммирования
    /// </summary>
    public enum enDirection : byte
    {
        none=0,
        Строки=1,
        Столбцы=2
    }

    public enum enImportItems
    {
        [Description("")]                                                   none = 0,
        [Description("Услуги, пациенты (до разложения)")]                   УслугиПациентыДоРазложения = 1,
        [Description("Услуги, пациенты, ошибки (после разложения)")]        УслугиПациентыОшибкиПослеРазложения = 2,
        [Description("Мед. персонал")]                                      МедПерсонал = 3,
        [Description("Классификатор услуг")]                                КлассификаторУслуг = 4,
        [Description("Сводный регистр застрахованных")]                     СРЗ = 5

    }

    public enum enImportStatus:byte
    {
        [Description("")]                               none = 0,
        [Description("Запущено")]                       Begin = 1,
        [Description("Завершено Успешно")]              End = 2,
        [Description("Ошибка")]          Failed = 3,
        [Description("Нет файла")]              FileNotFound = 4,
        [Description("Завершено Предупреждение")]    Warning = 5
    }

    public enum enImportTableNames : byte
    {
        [Description("")]                           none = 0,
        [Description("Пациенты")]                   Patient = 1,
        [Description("Услуги")]                     Service = 2,
        [Description("Ошибки")]                     Error = 3,
        [Description("Врачи")]                      Doctor = 4,
        [Description("Классификатор услуг")]        ServiceList = 5,
        [Description("Периодические осмотры")]      PreventiveExam = 6
    }



}