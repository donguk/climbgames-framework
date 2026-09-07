using System;
using System.Collections.Generic;
using System.Linq;

namespace ClimbGames
{
    public class CustomArgs
    {
        public static CustomArgs ParseCommandLineArgs()
        {
            string customArgs = Environment.GetCommandLineArgs().FirstOrDefault(arg => arg.StartsWith("-customArgs:"));
            return new CustomArgs(customArgs.Substring("-customArgs:".Length));
        }

        CustomArgs(string customArgs)
        {
            Parse(customArgs);
        }

        private Dictionary<string, string> args = new Dictionary<string, string>();

        public string this[string key]
        {
            get
            {
                if (args.TryGetValue(key, out var value))
                    return value;
                return string.Empty;
            }
        }

        void Parse(string customArgs)
        {
            string[] parameters = customArgs.Split(',');
            foreach (var parameter in parameters)
            {
                parameter.Trim();
                string[] values = parameter.Split(':');
                if (values.Length > 1)
                    args[values[0].Trim()] = values[1].Trim();
            }
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (!args.TryGetValue(key, out var value) || string.IsNullOrEmpty(value))
                return defaultValue;

            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            try
            {
                // 1. Enum 타입인 경우 Enum.Parse 처리
                if (targetType.IsEnum)
                {
                    return (T)Enum.Parse(targetType, value, ignoreCase: true);
                }

                // 2. 일반 기본 타입(int, float, string, bool 등) 변환
                return (T)Convert.ChangeType(value, targetType);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[ArgumentParser] '{key}' 키의 값 '{value}'를 {typeof(T).Name} 타입으로 변환 실패. 기본값({defaultValue})을 사용합니다. 예외: {ex.Message}");
                return defaultValue;
            }
        }
    }
}