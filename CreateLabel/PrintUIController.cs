namespace CreateLabel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal abstract class PrintUIController : UiController
    {
        public virtual string LabelForStylesheet => $"Stylesheet for {ApiName} Rendering";

        public virtual string LabelForResponseSchema => $"Validate {ApiName} Response Schema";

        public virtual string Stylesheet { get; set; } = "";

        public virtual string StylesheetPath { get; set; } = "";

        public virtual bool IsFOStylesheet { get; set; }

        public virtual string ResponseSchema { get; set; } = "";

        public virtual string ResponseSchemaPath { get; set; } = "";

        public virtual bool ValidateResponseSchema { get; set; }

        public virtual bool ShowDocument { get; set; }

        public abstract void DisplayDocument();
    }
}
