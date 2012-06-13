﻿/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/22/2008	brian.kuhn	Created SyndicationResourceSection Class
****************************************************************************/
using System;
using System.ComponentModel;
using System.Configuration;
using System.Security.Permissions;
using System.Web;

namespace Argotic.Configuration.Provider
{
    /// <summary>
    /// Defines configuration settings to support the infrastructure for configuring and managing syndication resource details. This class cannot be inherited.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public sealed class SyndicationResourceSection : ConfigurationSection
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the default provider configuration property for the section.
        /// </summary>
        private static readonly ConfigurationProperty configurationSectionDefaultProviderProperty   = new ConfigurationProperty("defaultProvider", typeof(System.String), "XmlSyndicationResourceProvider", new StringConverter(), new StringValidator(1), ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold the providers configuration property for the section.
        /// </summary>
        private static readonly ConfigurationProperty configurationSectionProvidersProperty         = new ConfigurationProperty("providers", typeof(ProviderSettingsCollection), null, ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold a collection of configuration properties for the section.
        /// </summary>
        private static ConfigurationPropertyCollection configurationSectionProperties               = new ConfigurationPropertyCollection();
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region SyndicationResourceSection()
        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationResourceSection"/> class.
        /// </summary>
        public SyndicationResourceSection()
        {
            //------------------------------------------------------------
            //	Initialize configuration section properties
            //------------------------------------------------------------
            configurationSectionProperties.Add(configurationSectionProvidersProperty);
            configurationSectionProperties.Add(configurationSectionDefaultProviderProperty);
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region DefaultProvider
        /// <summary>
        /// Gets or sets the name of the default provider that is used to manage syndication resources.
        /// </summary>
        /// <value>The name of a provider in <see cref="Providers"/>. The default is <b>XmlSyndicationResourceProvider</b>.</value>
        /// <remarks>
        ///     The <see cref="DefaultProvider"/> must match a named value in the <b>providers</b> subsection of the <b>syndication</b> section of the configuration file. 
        ///     An empty string ("") is not a valid value for the <see cref="DefaultProvider"/> property.
        /// </remarks>
        [ConfigurationProperty("defaultProvider", DefaultValue = "XmlSyndicationResourceProvider", Options = ConfigurationPropertyOptions.None)]
        [StringValidator(MinLength=1)]
        public string DefaultProvider
        {
            get
            {
                return (string)base[configurationSectionDefaultProviderProperty];
            }
            set
            {
                base[configurationSectionDefaultProviderProperty] = value;
            }
        }
        #endregion

        #region Providers
        /// <summary>
        /// Gets a <see cref="ProviderSettingsCollection"/> object of <see cref="ProviderSettings"/> objects.
        /// </summary>
        /// <value>
        ///     A <see cref="ProviderSettingsCollection"/> that contains the provider's settings, defined within 
        ///     the <b>providers</b> subsection of the <b>syndication</b> section of the configuration file.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         Syndication resource providers are components that provide access to the syndicated content for an application. 
        ///         You can specify syndication resource providers in the <b>providers</b> subsection of the <b>syndication</b> section of the configuration file.
        ///     </para>
        ///     <para>
        ///         The <see cref="DefaultProvider"/> property contains the name of the provider that is used by default.
        ///     </para>
        /// </remarks>
        [ConfigurationProperty("providers", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public ProviderSettingsCollection Providers
        {
            get
            {
                return (ProviderSettingsCollection)base[configurationSectionProvidersProperty];
            }
        }
        #endregion

        //============================================================
        //	PROTECTED PROPERTIES
        //============================================================
        #region Properties
        /// <summary>
        /// Gets the configuration properties for this section.
        /// </summary>
        /// <value>A <see cref="ConfigurationPropertyCollection"/> object that represents the configuration properties for this section.</value>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return configurationSectionProperties;
            }
        }
        #endregion
    }
}
