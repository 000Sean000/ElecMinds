using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enums
{
	public enum ENodeClass
	{
		Basic, Group, Template, Instance, Database, Options, Selections
	}
	public enum ENoteComposition
	{
		Mixed, OnlyText, OnlySingleRef
	}
	public enum ENoteImportance
	{
		EssentialData, ContextualLabel
	}
	public enum ELinkType
	{
		RelateTo, RootIn, Aggregate, ComposedOf, Implement, NextIs, ReferTo
	}
	public enum ELinkInfoIndex
	{
		ArrowTail, ArrowBody, ArrowHead, UserDefLinkType
	}
	public enum EDereferencerType
	{
		Direct
	}
}
