using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HastlayerTimingTester
{

    namespace DataTypes
    {
        public abstract class Base
        {
            private int _size;
            Base(int size) { _size = size; }
            public abstract string DataTypeFromSize(int size);
            public abstract string GetFriendlyName(int size);
        }

        public abstract class ModifierBase
        {
            private Base _inputDataType;
            ModifierBase (Base inputDataType) { _inputDataType = inputDataType; }
            public abstract string DataTypeFromSize(int size);
            public abstract string GetFriendlyName(int size) { }
        }

        public class Unsigned : Base
        {
            public override string DataTypeFromSize(int size) =>
                string.Format("unsigned({0} downto 0)", size - 1);

            public override string GetFriendlyName(int size) =>
                string.Format("unsigned{0}", size);
        }

        public class Signed : Base
        {
            public override string DataTypeFromSize(int size) =>
                string.Format("signed({0} downto 0)", size - 1);

            public override string GetFriendlyName(int size) =>
                string.Format("signed{0}", size);
        }

        public class StdLogicVector : Base
        {
            public override string DataTypeFromSize(int size) =>
                string.Format("std_logic_vector({0} downto 0)", size - 1);

            public override string GetFriendlyName(int size) =>
                string.Format("std_logic_vector{0}", size);
        }

        /// <summary>Used if the output data type is the same as the input data type.</summary>
        public class SameOutput : Base
        {

        }
        public static string SameOutputDataType(
            int inputSize,
            DataTypes.Base inputDataType,
            bool getFriendlyName
        ) => inputDataType(inputSize, getFriendlyName);

        /// <summary>
        /// Used for operators that strictly have boolean as their output data type (like all comparison operators).
        /// </summary>
        public static string ComparisonWithBoolOutput(
            int inputSize,
            DataTypes.Base inputDataType,
            bool getFriendlyName
        ) => "boolean";

        /// <summary>
        /// Used for operators whose output is the same type as their input, but with
        /// double data size (e.g. multiplication).
        /// </summary>
        public static string DoubleSizedOutput(
            int inputSize,
            DataTypes.Base inputDataType,
            bool getFriendlyName
        ) => inputDataType(inputSize * 2, getFriendlyName);
    }
}
