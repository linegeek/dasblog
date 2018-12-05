﻿#region Copyright (c) 2003, newtelligence AG. All rights reserved.
/*
// Copyright (c) 2003, newtelligence AG. (http://www.newtelligence.com)
// Original BlogX Source Code: Copyright (c) 2003, Chris Anderson (http://simplegeek.com)
// All rights reserved.
//  
// Redistribution and use in source and binary forms, with or without modification, are permitted 
// provided that the following conditions are met: 
//  
// (1) Redistributions of source code must retain the above copyright notice, this list of 
// conditions and the following disclaimer. 
// (2) Redistributions in binary form must reproduce the above copyright notice, this list of 
// conditions and the following disclaimer in the documentation and/or other materials 
// provided with the distribution. 
// (3) Neither the name of the newtelligence AG nor the names of its contributors may be used 
// to endorse or promote products derived from this software without specific prior 
// written permission.
//      
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS 
// OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY 
// AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER 
// IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT 
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// -------------------------------------------------------------------------
//
// Original BlogX source code (c) 2003 by Chris Anderson (http://simplegeek.com)
// 
// newtelligence is a registered trademark of newtelligence Aktiengesellschaft.
// 
// For portions of this software, the some additional copyright notices may apply 
// which can either be found in the license.txt file included in the source distribution
// or following this notice. 
//
*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DasBlog.Core.Common.Comments
{
	/// <summary>
	/// Represesents a tag, which can be used in the comments.
	/// </summary>
	[XmlRoot("tag", Namespace = "urn:newtelligence-com:dasblog:config")]
	[XmlType(Namespace = "urn:newtelligence-com:dasblog:config")]
	public class ValidTag
	{

		// required for xml Serializer
		public ValidTag()
		{
			//...
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidTag"/> class.
		/// </summary>
		/// <param name="tagDefinition">The tag definition, defined as 'tag@att1@att2'.</param>
		public ValidTag(string tagDefinition)
		{

			if (tagDefinition == null || tagDefinition.Length == 0)
			{
				throw new ArgumentNullException("tagDefinition");
			}

			// tags are defined as tag@att1@att2
			// so splitting on @ should give us what we need
			string[] splitDef = tagDefinition.Split('@');

			// first item is the name
			this.name = splitDef[0];

			// check for attributes and copy to collection
			if (splitDef.Length == 1)
			{
				attributes = new string[0];
			}
			else
			{
				attributes = new string[splitDef.Length - 1];
				Array.Copy(splitDef, 1, attributes, 0, attributes.Length);
				Array.Sort(attributes, StringComparer.InvariantCultureIgnoreCase);
			}
		}

		/// <summary>
		/// Determines whether attribute with the specified attribute name is valid.
		/// </summary>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <returns>
		/// 	<see langword="true"/> if the attribute with the specified attribute name is valid; otherwise, <see langword="false"/>.
		/// </returns>
		public bool IsValidAttribute(string attributeName)
		{

			return Array.IndexOf(attributes, attributeName) >= 0;
		}

		public override string ToString()
		{
			return name + (attributes.Length > 0 ? "@" : "") + String.Join("@", attributes);
		}


		/// <summary>
		/// Gets the name of the tag.
		/// </summary>
		/// <value>The name of the tag.</value>
		[XmlAttribute("name", Namespace = "urn:newtelligence-com:dasblog:config")]
		public string Name
		{
			[DebuggerStepThrough]
			get
			{
				return this.name;
			}
			[DebuggerStepThrough]
			set
			{
				this.name = value;
			}
		}

		[XmlAttribute("attributes", Namespace = "urn:newtelligence-com:dasblog:config")]
		public string Attributes
		{
			[DebuggerStepThrough]
			get
			{
				return String.Join(",", attributes);
			}
			[DebuggerStepThrough]
			set
			{
				if (value == null || value.Length == 0)
				{
					attributes = new string[0];
				}
				else
				{
					attributes = value.Split(',');
				}
			}
		}

		[XmlAttribute("allowed", Namespace = "urn:newtelligence-com:dasblog:config")]
		public bool IsAllowed
		{
			[DebuggerStepThrough]
			get
			{
				return this.allowed;
			}
			[DebuggerStepThrough]
			set
			{
				this.allowed = value;
			}
		}

		// for versioning
		[XmlAnyElement]
		public XmlElement[] AnyElements;
		[XmlAnyAttribute]
		public XmlAttribute[] AnyAttributes;

		// FIELDS
		private bool allowed = true;
		private string name = "";
		private string[] attributes = new string[0];
	}
}
