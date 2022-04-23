using System;
using System.Collections.Generic;

namespace Left4Dead
{
    public class NormalZombieModel : Model
    {
        public NormalZombieModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }

        public NormalZombieModel(Model copy) : base(copy) { }

        public override char DisplayModel(int rotation) { return ModelData[rotation]; }

        public override string ToString()
        {
            return "Normal zombie model";
        }
    }
}
