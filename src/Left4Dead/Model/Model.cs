using System;
using System.Collections.Generic;

namespace Left4Dead
{
    public abstract class Model
    {
        protected char[] ModelData { get; set; }
        public ConsoleColor ModelBaseColor { get; set; }
        public ConsoleColor ModelColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }

        public Model(char[] model, ConsoleColor modelBaseColor, ConsoleColor backgroundColor)
        {
            this.ModelData = model;
            this.ModelBaseColor = modelBaseColor;
            this.ModelColor = modelBaseColor;
            this.BackgroundColor = backgroundColor;
        }

        public Model(Model copy)
        {
            ModelData = new char[4] { copy.ModelData[0], copy.ModelData[1], copy.ModelData[2], copy.ModelData[3] };
            ModelBaseColor = copy.ModelBaseColor;
            ModelColor = copy.ModelBaseColor;
            BackgroundColor = copy.BackgroundColor;
        }

        public abstract char DisplayModel(int rotation);

        public override bool Equals(object obj)
        {
            foreach (char c in ModelData)
            {
                if ((char)obj == c)
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = -1401498075;
            hashCode = hashCode * -1521134295 + EqualityComparer<char[]>.Default.GetHashCode(ModelData);
            hashCode = hashCode * -1521134295 + ModelBaseColor.GetHashCode();
            hashCode = hashCode * -1521134295 + ModelColor.GetHashCode();
            hashCode = hashCode * -1521134295 + BackgroundColor.GetHashCode();
            return hashCode;
        }
    }
}
