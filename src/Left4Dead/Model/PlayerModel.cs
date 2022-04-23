using System;

namespace Left4Dead
{
    public class PlayerModel : Model
    {
        public PlayerModel(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor) : base(model, modelBaseColor, backgroundColor) { }

        public PlayerModel(Model copy) : base(copy) { }

        public override char DisplayModel(int rotation) { return ModelData[rotation]; }

        public override string ToString()
        {
            return "Player model";
        }
    }
}
