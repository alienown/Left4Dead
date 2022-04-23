using System;

namespace Left4Dead
{
    public class HealthPackModel : Model
    {
        public HealthPackModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }

        public HealthPackModel(Model copy) : base(copy) { }

        public override char DisplayModel(int rotation) { return ModelData[rotation]; }

        public override string ToString()
        {
            return "Health pack model";
        }
    }
}
