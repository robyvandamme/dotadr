// Copyright © 2025 Roby Van Damme.

using DotAdr.Common;

namespace DotAdr.Commands
{
    internal interface IConfigurationService
    {
        /// <summary>
        /// Saves the ADR configuration.
        /// </summary>
        /// <param name="adrDirectory">The relative ADR directory path.</param>
        /// <param name="overwriteConfiguration">Overwrite the config if it exists.</param>
        void SaveAdrConfiguration(LocalDirectory adrDirectory, bool overwriteConfiguration);

        /// <summary>
        /// Gets DotBot config from the .bot directory.
        /// </summary>
        /// <returns><see cref="DotAdrConfig"/>The configuration.</returns>
        /// <exception cref="DotAdrException">When the config can not be found or has missing elements.</exception>
        DotAdrConfig GetDotAdrConfiguration();
    }
}
