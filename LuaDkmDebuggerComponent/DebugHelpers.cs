using System;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.DefaultPort;
using Microsoft.VisualStudio.Debugger.Native;

namespace LuaDkmDebuggerComponent
{
    static class DebugHelpers
    {
        internal static T GetOrCreateDataItem<T>(DkmDataContainer container) where T : DkmDataItem, new()
        {
            T item = container.GetDataItem<T>();

            if (item != null)
                return item;

            item = new T();

            container.SetDataItem<T>(DkmDataCreationDisposition.CreateNew, item);

            return item;
        }

        internal static ulong FindFunctionAddress(DkmRuntimeInstance runtimeInstance, string name)
        {
            foreach (var module in runtimeInstance.GetModuleInstances())
            {
                var address = (module as DkmNativeModuleInstance)?.FindExportName(name, IgnoreDataExports: true);

                if (address != null)
                    return address.CPUInstructionPart.InstructionPointer;
            }

            return 0;
        }

        internal static bool Is64Bit(DkmProcess process)
        {
            return (process.SystemInformation.Flags & DkmSystemInformationFlags.Is64Bit) != 0;
        }

        internal static int GetPointerSize(DkmProcess process)
        {
            return Is64Bit(process) ? 8 : 4;
        }

        internal static byte? ReadByteVariable(DkmProcess process, ulong address)
        {
            byte[] variableAddressData = new byte[1];

            try
            {
                if (process.ReadMemory(address, DkmReadMemoryFlags.None, variableAddressData) == 0)
                    return null;
            }
            catch (DkmException)
            {
                return null;
            }

            return variableAddressData[0];
        }

        internal static short? ReadShortVariable(DkmProcess process, ulong address)
        {
            byte[] variableAddressData = new byte[2];

            try
            {
                if (process.ReadMemory(address, DkmReadMemoryFlags.None, variableAddressData) == 0)
                    return null;
            }
            catch (DkmException)
            {
                return null;
            }

            return BitConverter.ToInt16(variableAddressData, 0);
        }

        internal static int? ReadIntVariable(DkmProcess process, ulong address)
        {
            byte[] variableAddressData = new byte[4];

            try
            {
                if (process.ReadMemory(address, DkmReadMemoryFlags.None, variableAddressData) == 0)
                    return null;
            }
            catch (DkmException)
            {
                return null;
            }

            return BitConverter.ToInt32(variableAddressData, 0);
        }

        internal static uint? ReadUintVariable(DkmProcess process, ulong address)
        {
            byte[] variableAddressData = new byte[4];

            try
            {
                if (process.ReadMemory(address, DkmReadMemoryFlags.None, variableAddressData) == 0)
                    return null;
            }
            catch (DkmException)
            {
                return null;
            }

            return BitConverter.ToUInt32(variableAddressData, 0);
        }

        internal static long? ReadLongVariable(DkmProcess process, ulong address)
        {
            byte[] variableAddressData = new byte[8];

            try
            {
                if (process.ReadMemory(address, DkmReadMemoryFlags.None, variableAddressData) == 0)
                    return null;
            }
            catch (DkmException)
            {
                return null;
            }

            return BitConverter.ToInt64(variableAddressData, 0);
        }

        internal static ulong? ReadUlongVariable(DkmProcess process, ulong address)
        {
            byte[] variableAddressData = new byte[8];

            try
            {
                if (process.ReadMemory(address, DkmReadMemoryFlags.None, variableAddressData) == 0)
                    return null;
            }
            catch (DkmException)
            {
                return null;
            }

            return BitConverter.ToUInt64(variableAddressData, 0);
        }

        internal static float? ReadFloatVariable(DkmProcess process, ulong address)
        {
            byte[] variableAddressData = new byte[4];

            try
            {
                if (process.ReadMemory(address, DkmReadMemoryFlags.None, variableAddressData) == 0)
                    return null;
            }
            catch (DkmException)
            {
                return null;
            }

            return BitConverter.ToSingle(variableAddressData, 0);
        }

        internal static double? ReadDoubleVariable(DkmProcess process, ulong address)
        {
            byte[] variableAddressData = new byte[8];

            try
            {
                if (process.ReadMemory(address, DkmReadMemoryFlags.None, variableAddressData) == 0)
                    return null;
            }
            catch (DkmException)
            {
                return null;
            }

            return BitConverter.ToDouble(variableAddressData, 0);
        }

        internal static ulong? ReadPointerVariable(DkmProcess process, ulong address)
        {
            if (!Is64Bit(process))
                return ReadUintVariable(process, address);

            return ReadUlongVariable(process, address);
        }

        internal static byte[] ReadRawStringVariable(DkmProcess process, ulong address, int limit)
        {
            try
            {
                return process.ReadMemoryString(address, DkmReadMemoryFlags.AllowPartialRead, 1, limit);
            }
            catch (DkmException)
            {
            }

            return null;
        }

        internal static string ReadStringVariable(DkmProcess process, ulong address, int limit)
        {
            try
            {
                byte[] nameData = process.ReadMemoryString(address, DkmReadMemoryFlags.AllowPartialRead, 1, limit);

                if (nameData != null && nameData.Length != 0)
                    return System.Text.Encoding.UTF8.GetString(nameData, 0, nameData.Length - 1);
            }
            catch (DkmException)
            {
                return null;
            }

            return null;
        }

        internal static ulong? ReadPointerVariable(DkmProcess process, string name)
        {
            var runtimeInstance = process.GetNativeRuntimeInstance();

            if (runtimeInstance != null)
            {
                foreach (var module in runtimeInstance.GetModuleInstances())
                {
                    var nativeModule = module as DkmNativeModuleInstance;

                    var variableAddress = nativeModule?.FindExportName(name, IgnoreDataExports: false);

                    if (variableAddress != null)
                        return ReadPointerVariable(process, variableAddress.CPUInstructionPart.InstructionPointer);
                }
            }

            return null;
        }

        internal static bool TryWriteRawBytes(DkmProcess process, ulong address, byte[] value)
        {
            try
            {
                process.WriteMemory(address, value);
            }
            catch (DkmException)
            {
                return false;
            }

            return true;
        }

        internal static bool TryWriteByteVariable(DkmProcess process, ulong address, byte value)
        {
            try
            {
                process.WriteMemory(address, new byte[1] { value });
            }
            catch (DkmException)
            {
                return false;
            }

            return true;
        }

        internal static bool TryWriteShortVariable(DkmProcess process, ulong address, short value)
        {
            try
            {
                process.WriteMemory(address, BitConverter.GetBytes(value));
            }
            catch (DkmException)
            {
                return false;
            }

            return true;
        }

        internal static bool TryWriteIntVariable(DkmProcess process, ulong address, int value)
        {
            try
            {
                process.WriteMemory(address, BitConverter.GetBytes(value));
            }
            catch (DkmException)
            {
                return false;
            }

            return true;
        }

        internal static bool TryWriteUintVariable(DkmProcess process, ulong address, uint value)
        {
            try
            {
                process.WriteMemory(address, BitConverter.GetBytes(value));
            }
            catch (DkmException)
            {
                return false;
            }

            return true;
        }

        internal static bool TryWriteLongVariable(DkmProcess process, ulong address, long value)
        {
            try
            {
                process.WriteMemory(address, BitConverter.GetBytes(value));
            }
            catch (DkmException)
            {
                return false;
            }

            return true;
        }

        internal static bool TryWriteUlongVariable(DkmProcess process, ulong address, ulong value)
        {
            try
            {
                process.WriteMemory(address, BitConverter.GetBytes(value));
            }
            catch (DkmException)
            {
                return false;
            }

            return true;
        }

        internal static bool TryWriteFloatVariable(DkmProcess process, ulong address, float value)
        {
            try
            {
                process.WriteMemory(address, BitConverter.GetBytes(value));
            }
            catch (DkmException)
            {
                return false;
            }

            return true;
        }

        internal static bool TryWriteDoubleVariable(DkmProcess process, ulong address, double value)
        {
            try
            {
                process.WriteMemory(address, BitConverter.GetBytes(value));
            }
            catch (DkmException)
            {
                return false;
            }

            return true;
        }

        internal static bool TryWritePointerVariable(DkmProcess process, ulong address, ulong value)
        {
            if (!Is64Bit(process))
                return TryWriteUintVariable(process, address, (uint)value);

            return TryWriteUlongVariable(process, address, value);
        }

        internal static byte? ReadStructByte(DkmProcess process, ref ulong address)
        {
            var result = ReadByteVariable(process, address);

            address += 1ul;

            return result;
        }

        internal static short? ReadStructShort(DkmProcess process, ref ulong address)
        {
            address = (address + 1ul) & ~1ul; // Align

            var result = ReadShortVariable(process, address);

            address += 2ul;

            return result;
        }

        internal static int? ReadStructInt(DkmProcess process, ref ulong address)
        {
            address = (address + 3ul) & ~3ul; // Align

            var result = ReadIntVariable(process, address);

            address += 4ul;

            return result;
        }

        internal static uint? ReadStructUint(DkmProcess process, ref ulong address)
        {
            address = (address + 3ul) & ~3ul; // Align

            var result = ReadUintVariable(process, address);

            address += 4ul;

            return result;
        }

        internal static long? ReadStructLong(DkmProcess process, ref ulong address)
        {
            address = (address + 7ul) & ~7ul; // Align

            var result = ReadLongVariable(process, address);

            address += 8ul;

            return result;
        }

        internal static ulong? ReadStructUlong(DkmProcess process, ref ulong address)
        {
            address = (address + 7ul) & ~7ul; // Align

            var result = ReadUlongVariable(process, address);

            address += 8ul;

            return result;
        }

        internal static ulong? ReadStructPointer(DkmProcess process, ref ulong address)
        {
            if (!Is64Bit(process))
                return ReadStructUint(process, ref address);

            return ReadStructUlong(process, ref address);
        }

        internal static void SkipStructByte(DkmProcess process, ref ulong address)
        {
            address += 1ul;
        }

        internal static void SkipStructShort(DkmProcess process, ref ulong address)
        {
            address = (address + 1ul) & ~1ul; // Align

            address += 2ul;
        }

        internal static void SkipStructInt(DkmProcess process, ref ulong address)
        {
            address = (address + 3ul) & ~3ul; // Align

            address += 4ul;
        }

        internal static void SkipStructUint(DkmProcess process, ref ulong address)
        {
            address = (address + 3ul) & ~3ul; // Align

            address += 4ul;
        }

        internal static void SkipStructLong(DkmProcess process, ref ulong address)
        {
            address = (address + 7ul) & ~7ul; // Align

            address += 8ul;
        }

        internal static void SkipStructUlong(DkmProcess process, ref ulong address)
        {
            address = (address + 7ul) & ~7ul; // Align

            address += 8ul;
        }

        internal static void SkipStructPointer(DkmProcess process, ref ulong address)
        {
            if (!Is64Bit(process))
                SkipStructUint(process, ref address);
            else
                SkipStructUlong(process, ref address);
        }
    }
}
