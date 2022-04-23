using System;

namespace Left4Dead
{
    public class HunterZombieModel : Model
    {
        public HunterZombieModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }

        public HunterZombieModel(Model copy) : base(copy) { }

        public override char DisplayModel(int rotation) { return ModelData[rotation]; }

        public override string ToString()
        {
            return "Hunter zombie model";
        }
    }
}
