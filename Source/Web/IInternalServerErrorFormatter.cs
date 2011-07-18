using System;

namespace Concoct.Web
{
    public interface IInternalServerErrorFormatter
    {
        string Format(Exception e);
    }

    class BasicInternalErrorFormatter : IInternalServerErrorFormatter
    {
        string IInternalServerErrorFormatter.Format(Exception e) {
            return string.Format("<pre>{0}</pre>", e);
        }
    }
}
