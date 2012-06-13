﻿/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/31/2008	brian.kuhn	Created LiveJournalSecurity Class
****************************************************************************/
using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents the access level of a LiveJournal entry.
    /// </summary>
    /// <seealso cref="LiveJournalSyndicationExtensionContext.Security"/>
    [Serializable()]
    public class LiveJournalSecurity : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold security type indicator.
        /// </summary>
        private LiveJournalSecurityType securityType    = LiveJournalSecurityType.Public;
        /// <summary>
        /// Private member to hold an integer indicating the friend-groups mask.
        /// </summary>
        private int securityMask                        = Int32.MinValue;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region LiveJournalSecurity()
        /// <summary>
        /// Initializes a new instance of the <see cref="LiveJournalSecurity"/> class.
        /// </summary>
        public LiveJournalSecurity()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region LiveJournalSecurity(LiveJournalSecurityType accessType)
        /// <summary>
        /// Initializes a new instance of the <see cref="LiveJournalSecurity"/> class using the supplied <see cref="LiveJournalSecurityType"/>.
        /// </summary>
        /// <param name="accessType">A <see cref="LiveJournalSecurityType"/> enumeration value that represents the access type.</param>
        public LiveJournalSecurity(LiveJournalSecurityType accessType)
        {
            //------------------------------------------------------------
            //	Initialize class state using property setters
            //------------------------------------------------------------
            this.Accessibility  = accessType;
        }
        #endregion

        #region LiveJournalSecurity(LiveJournalSecurityType accessType, int mask)
        /// <summary>
        /// Initializes a new instance of the <see cref="LiveJournalSecurity"/> class using the supplied <see cref="LiveJournalSecurityType"/>.
        /// </summary>
        /// <param name="accessType">A <see cref="LiveJournalSecurityType"/> enumeration value that represents the access type.</param>
        /// <param name="mask">An integer indicating the friend-groups mask.</param>
        public LiveJournalSecurity(LiveJournalSecurityType accessType, int mask)
        {
            //------------------------------------------------------------
            //	Initialize class state using property setters
            //------------------------------------------------------------
            this.Accessibility  = accessType;
            this.Mask           = mask;
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Accessibility
        /// <summary>
        /// Gets or sets the accessibility type.
        /// </summary>
        /// <value>A <see cref="LiveJournalSecurityType"/> enumeration value that represents the access type. The default value is <see cref="LiveJournalSecurityType.Public"/>.</value>
        public LiveJournalSecurityType Accessibility
        {
            get
            {
                return securityType;
            }

            set
            {
                securityType = value;
            }
        }
        #endregion

        #region Mask
        /// <summary>
        /// Gets or sets the friend-groups mask.
        /// </summary>
        /// <value>An integer indicating the friend-groups mask used. The default value is <see cref="Int32.MinValue"/>, which indicates that no friend-groups mask was specified.</value>
        /// <remarks>
        ///     This property only applies if the <see cref="Accessibility"/> property is <see cref="LiveJournalSecurityType.Friends">friends</see> 
        ///     and <b>only</b> if the author of the post is the same as the user who has authenticated the feed request.
        /// </remarks>
        public int Mask
        {
            get
            {
                return securityMask;
            }

            set
            {
                securityMask = value;
            }
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region AccessibilityAsString(LiveJournalSecurityType level)
        /// <summary>
        /// Returns the access level identifier for the supplied <see cref="LiveJournalSecurityType"/>.
        /// </summary>
        /// <param name="level">The <see cref="LiveJournalSecurityType"/> to get the access level identifier for.</param>
        /// <returns>The access level identifier for the supplied <paramref name="level"/>, otherwise returns an empty string.</returns>
        public static string AccessibilityAsString(LiveJournalSecurityType level)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string name = String.Empty;

            //------------------------------------------------------------
            //	Return alternate value based on supplied protocol
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(LiveJournalSecurityType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(LiveJournalSecurityType))
                {
                    LiveJournalSecurityType accessLevel = (LiveJournalSecurityType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (accessLevel == level)
                    {
                        object[] customAttributes   = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                            name    = enumerationMetadata.AlternateValue;
                            break;
                        }
                    }
                }
            }

            return name;
        }
        #endregion

        #region AccessibilityByName(string name)
        /// <summary>
        /// Returns the <see cref="LiveJournalSecurityType"/> enumeration value that corresponds to the specified access level name.
        /// </summary>
        /// <param name="name">The name of the access level.</param>
        /// <returns>A <see cref="LiveJournalSecurityType"/> enumeration value that corresponds to the specified string, otherwise returns <b>LiveJournalSecurityType.None</b>.</returns>
        /// <remarks>This method disregards case of specified access level name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static LiveJournalSecurityType AccessibilityByName(string name)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            LiveJournalSecurityType accessLevel = LiveJournalSecurityType.None;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(name, "name");

            //------------------------------------------------------------
            //	Determine syndication content format based on supplied name
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(LiveJournalSecurityType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(LiveJournalSecurityType))
                {
                    LiveJournalSecurityType level   = (LiveJournalSecurityType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes       = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (String.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            accessLevel = level;
                            break;
                        }
                    }
                }
            }

            return accessLevel;
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source)
        /// <summary>
        /// Loads this <see cref="LiveJournalSecurity"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="LiveJournalSecurity"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="LiveJournalSecurity"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded              = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            if(source.HasAttributes)
            {
                string typeAttribute    = source.GetAttribute("type", String.Empty);
                string maskAttribute    = source.GetAttribute("mask", String.Empty);

                if (!String.IsNullOrEmpty(typeAttribute))
                {
                    LiveJournalSecurityType accessLevel = LiveJournalSecurity.AccessibilityByName(typeAttribute);
                    if (accessLevel != LiveJournalSecurityType.None)
                    {
                        this.Accessibility  = accessLevel;
                        wasLoaded           = true;
                    }
                }

                if (!String.IsNullOrEmpty(maskAttribute))
                {
                    int mask;
                    if (Int32.TryParse(maskAttribute, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out mask))
                    {
                        this.Mask   = mask;
                        wasLoaded   = true;
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="LiveJournalSecurity"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");

            //------------------------------------------------------------
            //	Create extension instance to retrieve XML namespace
            //------------------------------------------------------------
            LiveJournalSyndicationExtension extension   = new LiveJournalSyndicationExtension();

            //------------------------------------------------------------
            //	Write XML representation of the current instance
            //------------------------------------------------------------
            writer.WriteStartElement("security", extension.XmlNamespace);

            writer.WriteAttributeString("type", LiveJournalSecurity.AccessibilityAsString(this.Accessibility));
            
            if(this.Mask != Int32.MinValue)
            {
                writer.WriteAttributeString("mask", this.Mask.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }

            writer.WriteEndElement();
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="LiveJournalSecurity"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="LiveJournalSecurity"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            //------------------------------------------------------------
            //	Build the string representation
            //------------------------------------------------------------
            using(MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings  = new XmlWriterSettings();
                settings.ConformanceLevel   = ConformanceLevel.Fragment;
                settings.Indent             = true;
                settings.OmitXmlDeclaration = true;

                using(XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    this.WriteTo(writer);
                }

                stream.Seek(0, SeekOrigin.Begin);

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        #endregion

        //============================================================
        //	ICOMPARABLE IMPLEMENTATION
        //============================================================
        #region CompareTo(object obj)
        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
        public int CompareTo(object obj)
        {
            //------------------------------------------------------------
            //	If target is a null reference, instance is greater
            //------------------------------------------------------------
            if (obj == null)
            {
                return 1;
            }

            //------------------------------------------------------------
            //	Determine comparison result using property state of objects
            //------------------------------------------------------------
            LiveJournalSecurity value  = obj as LiveJournalSecurity;

            if (value != null)
            {
                int result  = this.Accessibility.CompareTo(value.Accessibility);
                result      = this.Mask.CompareTo(value.Mask);

                return result;
            }
            else
            {
                throw new ArgumentException(String.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
        }
        #endregion

        #region Equals(Object obj)
        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="Object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(Object obj)
        {
            //------------------------------------------------------------
            //	Determine equality via type then by comparision
            //------------------------------------------------------------
            if (!(obj is LiveJournalSecurity))
            {
                return false;
            }

            return (this.CompareTo(obj) == 0);
        }
        #endregion

        #region GetHashCode()
        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            //------------------------------------------------------------
            //	Generate has code using unique value of ToString() method
            //------------------------------------------------------------
            char[] charArray    = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }
        #endregion

        #region == operator
        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(LiveJournalSecurity first, LiveJournalSecurity second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return true;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return first.Equals(second);
        }
        #endregion

        #region != operator
        /// <summary>
        /// Determines if operands are not equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
        public static bool operator !=(LiveJournalSecurity first, LiveJournalSecurity second)
        {
            return !(first == second);
        }
        #endregion

        #region < operator
        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(LiveJournalSecurity first, LiveJournalSecurity second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return true;
            }

            return (first.CompareTo(second) < 0);
        }
        #endregion

        #region > operator
        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(LiveJournalSecurity first, LiveJournalSecurity second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return (first.CompareTo(second) > 0);
        }
        #endregion
    }
}
