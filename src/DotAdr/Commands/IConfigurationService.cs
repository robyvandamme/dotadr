// Copyright © 2025 Roby Van Damme.

using DotAdr.Common;

namespace DotAdr.Commands
{
    internal interface IConfigurationService
    {
        void SaveAdrConfiguration(LocalDirectory adrDirectory, bool overwriteConfiguration);

        DotAdrConfig GetDotAdrConfiguration();
    }
}
