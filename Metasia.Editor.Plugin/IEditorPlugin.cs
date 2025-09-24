namespace Metasia.Editor.Plugin
{
    public interface IEditorPlugin
    {
        /// <summary>
        /// エディタプラグインの識別子
        /// </summary>
        string PluginIdentifier { get; }

        /// <summary>
        /// エディタプラグインのバージョン MAJOR.MINOR.PATCH形式を推奨
        /// </summary>
        string PluginVersion { get; }

        /// <summary>
        /// エディタプラグインの名前
        /// </summary>
        string PluginName { get; }

        public enum SupportEnvironment
        {
            Windows_x64,
            Windows_arm64,
            MacOS_x64,
            MacOS_arm64,
            Linux_x64,
            Linux_arm64,
        }
        /// <summary>
        /// エディタプラグインがサポートする環境
        /// </summary>
        IEnumerable<SupportEnvironment> SupportedEnvironments { get; }

    }
}
