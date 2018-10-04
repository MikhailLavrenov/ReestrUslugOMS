using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReestrUslugOMS
{
    /// <summary>
    /// Класс возвращаемого значение хранимой процедуры sp_ReportFact
    /// </summary>
    public partial class sp_ReportFactResult
    {

        /// <summary>
        /// Возвращает значение соответствующее значению перечисления enDataType
        /// </summary>
        /// <param name="enu">Значение перечисления</param>
        /// <returns>Возвращаемое значение</returns>
        public string GetValue (enDataType enu)
        {
            string result = null;

            if (enu== enDataType.КодВрача)
                result = CodDoc;
            else if (enu== enDataType.КодОтделения)
                result = CodOtd;
            else if (enu == enDataType.КодУслуги)
                result = CodUsl;

            return result;
        }

        /// <summary>
        /// Возвращает значение соответствующее значению перечисления enResultType
        /// </summary>
        /// <param name="enu">Значение перечисления</param>
        /// <returns>Возвращаемое значение</returns>
        public double GetValue(enResultType enu)
        {
            double result=0;

            if (enu == enResultType.Услуг)
                result = (double)Usl;
            else if (enu == enResultType.КоличествоУслуг)
                result = (double)ColUsl;
            else if (enu == enResultType.УЕТ)
                result = (double)UET;
            else if (enu == enResultType.УслугДо3Лет)
                result = (double)UslDo3;
            else if (enu == enResultType.УслугПосле3Лет)
                result = (double)UslPosle3;
            else if (enu == enResultType.ДиспансеризацияРазВ2Года)
                result = (double)DispIn2Y;

            return result;
        }

    }
}
