
using System.ComponentModel;

namespace ReestrUslugOMS
{


    public enum enDataSource:byte
    {
        [Description("")]                           Пусто = 0,
        [Description("Реестры-счетов")]             РеестрыСчетов = 1,
        [Description("Отчет")]                      Отчет = 2,
        [Description("План Отделения")]             ПланОтделения = 3,
        [Description("План Врача")]                 ПланВрача = 4
    }

    public enum enDataType : byte
    {
        [Description("")]                           Пусто = 0,
        [Description("Код Врача")]                  КодВрача = 1,
        [Description("Код Отделения")]              КодОтделения = 2,
        [Description("Код Услуги")]                 КодУслуги = 3,
        [Description("Название Элемента")]          НазваниеЭлемента = 4,
        [Description("Номер Элемента")]             НомерЭлемента = 5,
    }

    public enum enOperation : byte
    {
        [Description("")]                           Пусто = 0,
        [Description("Умножение")]                  Умножение = 1,
        [Description("Деление")]                    Деление = 2,
        [Description("Умножение Выборочно")]        УмножениеВыборочно = 3,
        [Description("Не применять Выборочно")]     НеПрименятьВыборочно = 4,
    }

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

    }
    public enum enInsuranceMode : byte
    {
        [Description("")]                           Пусто = 0,
        [Description("Все пациенты")]               ВсеПациенты = 1,
        [Description("Без иногородних")]            БезИногодних = 2,
        [Description("Только иногородние")]         ТолькоИногородние = 3
    }

    public enum enErrorMode : byte
    {
        [Description("")]                           Пусто = 0,
        [Description("Все услуги")]                 ВсеУслуги = 1,
        [Description("Без ошибок")]                 БезОшибок = 2,
        [Description("Только ошибки")]              ТолькоОшибки = 3
    }

    public enum enReportMode :byte
    {
        none = 0,        
        ПланВрача = 1,
        ПланОтделения = 2,
        Отчет = 3
    }
    public enum enDirection : byte
    {
        none=0,
        Строки=1,
        Столбцы=2
    }
  
    
    
   
}