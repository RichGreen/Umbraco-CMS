﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using umbraco.interfaces;
using System.Reflection;
using System.Xml.Linq;

namespace Umbraco.Core.Dynamics
{
	internal class DynamicDocument : DynamicObject
	{
		#region consts
		// these are private readonlys as const can't be Guids
		private readonly Guid DATATYPE_YESNO_GUID = new Guid("38b352c1-e9f8-4fd8-9324-9a2eab06d97a");
		private readonly Guid DATATYPE_TINYMCE_GUID = new Guid("5e9b75ae-face-41c8-b47e-5f4b0fd82f83");
		private readonly Guid DATATYPE_DATETIMEPICKER_GUID = new Guid("b6fb1622-afa5-4bbf-a3cc-d9672a442222");
		private readonly Guid DATATYPE_DATEPICKER_GUID = new Guid("23e93522-3200-44e2-9f29-e61a6fcbb79a");
		//private readonly Guid DATATYPE_INTEGER_GUID = new Guid("1413afcb-d19a-4173-8e9a-68288d2a73b8");
		#endregion

		private readonly DynamicBackingItem _backingItem;
		private DynamicDocumentList _cachedChildren;

		internal DynamicDocumentList OwnerList { get; set; }

		public DynamicDocument(DynamicBackingItem n)
		{
			if (n == null) throw new ArgumentNullException("n");
			_backingItem = n;
		}

		//public DynamicNode(int nodeId)
		//{
		//    this._n = new DynamicBackingItem(nodeId);
		//}
		//public DynamicNode(int nodeId, DynamicBackingItemType itemType)
		//{
		//    this._n = new DynamicBackingItem(nodeId, itemType);
		//}
		//public DynamicNode(string nodeId)
		//{
		//    int DynamicBackingItemId = 0;
		//    if (int.TryParse(nodeId, out DynamicBackingItemId))
		//    {
		//        this._n = new DynamicBackingItem(DynamicBackingItemId);
		//        return;
		//    }
		//    throw new ArgumentException("Cannot instantiate a DynamicNode without an id");
		//}
		public DynamicDocument(IDocument node)
		{
			if (node == null) throw new ArgumentNullException("node");
			_backingItem = new DynamicBackingItem(node);
		}

		//public DynamicNode(object nodeId)
		//{
		//    var dynamicBackingItemId = 0;
		//    if (int.TryParse(string.Format("{0}", nodeId), out dynamicBackingItemId))
		//    {
		//        this._n = new DynamicBackingItem(dynamicBackingItemId);
		//        return;
		//    }
		//    throw new ArgumentException("Cannot instantiate a DynamicNode without an id");
		//}
		public DynamicDocument()
		{
			//Empty constructor for a special case with Generic Methods
		}

		public dynamic AsDynamic()
		{
			return this;
		}

		public DynamicDocument Up()
		{
			return DynamicDocumentWalker.Up(this);
		}
		public DynamicDocument Up(int number)
		{
			return DynamicDocumentWalker.Up(this, number);
		}
		public DynamicDocument Up(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Up(this, nodeTypeAlias);
		}
		public DynamicDocument Down()
		{
			return DynamicDocumentWalker.Down(this);
		}
		public DynamicDocument Down(int number)
		{
			return DynamicDocumentWalker.Down(this, number);
		}
		public DynamicDocument Down(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Down(this, nodeTypeAlias);
		}
		public DynamicDocument Next()
		{
			return DynamicDocumentWalker.Next(this);
		}
		public DynamicDocument Next(int number)
		{
			return DynamicDocumentWalker.Next(this, number);
		}
		public DynamicDocument Next(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Next(this, nodeTypeAlias);
		}

		public DynamicDocument Previous()
		{
			return DynamicDocumentWalker.Previous(this);
		}
		public DynamicDocument Previous(int number)
		{
			return DynamicDocumentWalker.Previous(this, number);
		}
		public DynamicDocument Previous(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Previous(this, nodeTypeAlias);
		}
		public DynamicDocument Sibling(int number)
		{
			return DynamicDocumentWalker.Sibling(this, number);
		}
		public DynamicDocument Sibling(string nodeTypeAlias)
		{
			return DynamicDocumentWalker.Sibling(this, nodeTypeAlias);
		}

		//public DynamicNodeList XPath(string xPath)
		//{
		//    //if this DN was initialized with an underlying NodeFactory.Node
		//    if (n != null && n.Type == DynamicBackingItemType.Content)
		//    {
		//        //get the underlying xml content
		//        XmlDocument doc = umbraco.content.Instance.XmlContent;
		//        if (doc != null)
		//        {
		//            //get n as a XmlNode (to be used as the context point for the xpath)
		//            //rather than just applying the xPath to the root node, this lets us use .. etc from the DynamicNode point


		//            //in test mode, n.Id is 0, let this always succeed
		//            if (n.Id == 0)
		//            {
		//                List<DynamicNode> selfList = new List<DynamicNode>() { this };
		//                return new DynamicNodeList(selfList);
		//            }
		//            XmlNode node = doc.SelectSingleNode(string.Format("//*[@id='{0}']", n.Id));
		//            if (node != null)
		//            {
		//                //got the current node (within the XmlContent instance)
		//                XmlNodeList nodes = node.SelectNodes(xPath);
		//                if (nodes.Count > 0)
		//                {
		//                    //we got some resulting nodes
		//                    List<NodeFactory.Node> nodeFactoryNodeList = new List<NodeFactory.Node>();
		//                    //attempt to convert each node in the set to a NodeFactory.Node
		//                    foreach (XmlNode nodeXmlNode in nodes)
		//                    {
		//                        try
		//                        {
		//                            nodeFactoryNodeList.Add(new NodeFactory.Node(nodeXmlNode));
		//                        }
		//                        catch (Exception) { } //swallow the exceptions - the returned nodes might not be full nodes, e.g. property
		//                    }
		//                    //Wanted to do this, but because we return DynamicNodeList here, the only
		//                    //common parent class is DynamicObject
		//                    //maybe some future refactoring will solve this?
		//                    //if (nodeFactoryNodeList.Count == 0)
		//                    //{
		//                    //    //if the xpath resulted in a node set, but none of them could be converted to NodeFactory.Node
		//                    //    XElement xElement = XElement.Parse(node.OuterXml);
		//                    //    //return 
		//                    //    return new DynamicXml(xElement);
		//                    //}
		//                    //convert the NodeFactory nodelist to IEnumerable<DynamicNode> and return it as a DynamicNodeList
		//                    return new DynamicNodeList(nodeFactoryNodeList.ConvertAll(nfNode => new DynamicNode((INode)nfNode)));
		//                }
		//                else
		//                {
		//                    // XPath returned no nodes, return an empty DynamicNodeList
		//                    return new DynamicNodeList();
		//                }
		//            }
		//            else
		//            {
		//                throw new NullReferenceException("Couldn't locate the DynamicNode within the XmlContent");
		//            }
		//        }
		//        else
		//        {
		//            throw new NullReferenceException("umbraco.content.Instance.XmlContent is null");
		//        }
		//    }
		//    else
		//    {
		//        throw new NullReferenceException("DynamicNode wasn't initialized with an underlying NodeFactory.Node");
		//    }
		//}


		//public DynamicNodeList Search(string term, bool useWildCards = true, string searchProvider = null)
		//{
		//    var searcher = Examine.ExamineManager.Instance.DefaultSearchProvider;
		//    if(!string.IsNullOrEmpty(searchProvider))
		//        searcher = Examine.ExamineManager.Instance.SearchProviderCollection[searchProvider];

		//    var t = term.Escape().Value;
		//    if (useWildCards)
		//        t = term.MultipleCharacterWildcard().Value;

		//    string luceneQuery = "+__Path:(" + this.Path.Replace("-", "\\-") + "*) +" + t;
		//    var crit = searcher.CreateSearchCriteria().RawQuery(luceneQuery);

		//    return Search(crit, searcher);
		//}

		//public DynamicNodeList SearchDescendants(string term, bool useWildCards = true, string searchProvider = null)
		//{
		//    return Search(term, useWildCards, searchProvider);
		//}

		//public DynamicNodeList SearchChildren(string term, bool useWildCards = true, string searchProvider = null)
		//{
		//    var searcher = Examine.ExamineManager.Instance.DefaultSearchProvider;
		//    if (!string.IsNullOrEmpty(searchProvider))
		//        searcher = Examine.ExamineManager.Instance.SearchProviderCollection[searchProvider];

		//    var t = term.Escape().Value;
		//    if (useWildCards)
		//        t = term.MultipleCharacterWildcard().Value;

		//    string luceneQuery = "+parentID:" + this.Id.ToString() + " +" + t;
		//    var crit = searcher.CreateSearchCriteria().RawQuery(luceneQuery);

		//    return Search(crit, searcher);
		//}


		//public DynamicNodeList Search(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
		//{
		//    var s = Examine.ExamineManager.Instance.DefaultSearchProvider;
		//    if (searchProvider != null)
		//        s = searchProvider;

		//    var results = s.Search(criteria);
		//    return ExamineSearchUtill.convertSearchResultToDynamicNode(results);
		//}




		public bool HasProperty(string name)
		{
			if (_backingItem != null)
			{
				try
				{
					var prop = _backingItem.GetUserProperty(name);

					return (prop != null);
				}
				catch (Exception)
				{
					return false;
				}
			}
			return false;
		}
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			try
			{
				//Property?
				result = typeof(DynamicDocument).InvokeMember(binder.Name,
												  System.Reflection.BindingFlags.Instance |
												  System.Reflection.BindingFlags.Public |
												  System.Reflection.BindingFlags.NonPublic |
												  System.Reflection.BindingFlags.GetProperty,
												  null,
												  this,
												  args);
				return true;
			}
			catch (MissingMethodException)
			{
				try
				{
					//Static or Instance Method?
					result = typeof(DynamicDocument).InvokeMember(binder.Name,
												  System.Reflection.BindingFlags.Instance |
												  System.Reflection.BindingFlags.Public |
												  System.Reflection.BindingFlags.NonPublic |
												  System.Reflection.BindingFlags.Static |
												  System.Reflection.BindingFlags.InvokeMethod,
												  null,
												  this,
												  args);
					return true;
				}
				catch (MissingMethodException)
				{
					try
					{
						result = ExecuteExtensionMethod(args, binder.Name, false);
						return true;
					}
					catch (TargetInvocationException)
					{
						result = new DynamicNull();
						return true;
					}

					catch
					{
						result = null;
						return false;
					}

				}


			}
			catch
			{
				result = null;
				return false;
			}

		}

		private object ExecuteExtensionMethod(object[] args, string name, bool argsContainsThis)
		{
			object result = null;

			MethodInfo methodToExecute = ExtensionMethodFinder.FindExtensionMethod(typeof(IEnumerable<DynamicDocument>), args, name, false);
			if (methodToExecute == null)
			{
				methodToExecute = ExtensionMethodFinder.FindExtensionMethod(typeof(DynamicDocumentList), args, name, false);
			}
			if (methodToExecute != null)
			{
				var genericArgs = (new[] { this }).Concat(args);
				result = methodToExecute.Invoke(null, genericArgs.ToArray());
			}
			else
			{
				throw new MissingMethodException();
			}
			if (result != null)
			{
				//if (result is IEnumerable<DynamicBackingItem>)
				//{
				//    result = new DynamicNodeList((IEnumerable<DynamicBackingItem>)result);
				//}
				if (result is IEnumerable<DynamicDocument>)
				{
					result = new DynamicDocumentList((IEnumerable<DynamicDocument>)result);
				}
				if (result is DynamicBackingItem)
				{
					result = new DynamicDocument((DynamicBackingItem)result);
				}
			}
			return result;
		}

		//private static Dictionary<System.Tuple<Guid, int>, Type> _razorDataTypeModelTypes = null;
		//private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

		//internal static Dictionary<System.Tuple<Guid, int>, Type> RazorDataTypeModelTypes
		//{
		//    get
		//    {
		//        using (var l = new UpgradeableReadLock(Locker))
		//        {
		//            if (_razorDataTypeModelTypes == null)
		//            {
		//                l.UpgradeToWriteLock();

		//                var foundTypes = new Dictionary<System.Tuple<Guid, int>, Type>();

		//                //HttpContext.Current.Trace.Write("RazorDataTypeModelTypes cache is empty, populating cache using PluginTypeResolver...");
		//                try
		//                {
		//                    PluginManager.Current.ResolveRazorDataTypeModels()
		//                        .ToList()
		//                        .ConvertAll(type =>
		//                        {
		//                            var razorDataTypeModelAttributes = type.GetCustomAttributes<RazorDataTypeModel>(true);
		//                            return razorDataTypeModelAttributes.ToList().ConvertAll(razorDataTypeModelAttribute =>
		//                            {
		//                                var g = razorDataTypeModelAttribute.DataTypeEditorId;
		//                                var priority = razorDataTypeModelAttribute.Priority;
		//                                return new KeyValuePair<System.Tuple<Guid, int>, Type>(new System.Tuple<Guid, int>(g, priority), type);
		//                            });
		//                        })
		//                        .SelectMany(item => item)
		//                        .ToList()
		//                        .ForEach(item =>
		//                        {
		//                            System.Tuple<Guid, int> key = item.Key;
		//                            if (!foundTypes.ContainsKey(key))
		//                            {
		//                                foundTypes.Add(key, item.Value);
		//                            }
		//                        });
		//                    //HttpContext.Current.Trace.Write(string.Format("{0} items added to cache...", foundTypes.Count));
		//                    var i = 1;
		//                    foreach (var item in foundTypes)
		//                    {
		//                        HttpContext.Current.Trace.Write(string.Format("{0}/{1}: {2}@{4} => {3}", i, foundTypes.Count, item.Key.Item1, item.Value.FullName, item.Key.Item2));
		//                        i++;
		//                    }

		//                    //there is no error, so set the collection
		//                    _razorDataTypeModelTypes = foundTypes;

		//                }
		//                catch (Exception ex)
		//                {
		//                    //HttpContext.Current.Trace.Warn("Exception occurred while populating cache, will keep RazorDataTypeModelTypes to null so that this error remains visible and you don't end up with an empty cache with silent failure.");
		//                    //HttpContext.Current.Trace.Warn(string.Format("The exception was {0} and the message was {1}. {2}", ex.GetType().FullName, ex.Message, ex.StackTrace));							
		//                }

		//            }
		//            return _razorDataTypeModelTypes;
		//        }
		//    }
		//}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (binder == null) throw new ArgumentNullException("binder");

			var name = binder.Name;
			result = null; //this will never be returned

			if (name.InvariantEquals("ChildrenAsList") || name.InvariantEquals("Children"))
			{
				result = Children;
				return true;
			}
			if (name.InvariantEquals("parentId"))
			{
				var parent = Parent;
				if (parent == null)
				{
					throw new InvalidOperationException(string.Format("The node {0} does not have a parent", Id));
				}
				result = parent.Id;
				return true;
			}

			var recursive = false;
			if (name.StartsWith("_"))
			{
				name = name.Substring(1, name.Length - 1);
				recursive = true;
			}
			var userProperty = _backingItem.GetUserProperty(name, recursive);

			//check for a user defined property

			if (userProperty != null)
			{
				result = userProperty.Value;
				//special casing for true/false properties
				//int/decimal are handled by ConvertPropertyValueByDataType
				//fallback is stringT
				if (_backingItem.NodeTypeAlias == null && userProperty.Alias == null)
				{
					throw new InvalidOperationException("No node alias or property alias available. Unable to look up the datatype of the property you are trying to fetch.");
				}

				//contextAlias is the node which the property data was returned from
				var dataType = DynamicDocumentDataSourceResolver.Current.DataSource.GetDataType(userProperty.DocumentTypeAlias, userProperty.Alias);
				//HttpContext.Current.Trace.Write(string.Format("RazorDynamicNode got datatype {0} for {1} on {2}", dataType, data.Alias, data.ContextAlias));

				//HttpContext.Current.Trace.Write(string.Format("Checking for a RazorDataTypeModel for data type guid {0}...", dataType));
				//HttpContext.Current.Trace.Write("Checking the RazorDataTypeModelTypes static mappings to see if there is a static mapping...");

				//var staticMapping = UmbracoSettings.RazorDataTypeModelStaticMapping.FirstOrDefault(mapping =>
				//{
				//    return mapping.Applies(dataType, data.ContextAlias, data.Alias);
				//});
				//if (staticMapping != null)
				//{
				//    //HttpContext.Current.Trace.Write(string.Format("Found a staticMapping defined {0}, instantiating type and attempting to apply model...", staticMapping.Raw));
				//    Type dataTypeType = Type.GetType(staticMapping.TypeName);
				//    if (dataTypeType != null)
				//    {
				//        object instance = null;
				//        if (TryCreateInstanceRazorDataTypeModel(dataType, dataTypeType, data.Value, out instance))
				//        {
				//            result = instance;
				//            return true;
				//        }
				//        else
				//        {
				//            //HttpContext.Current.Trace.Write("Failed");
				//            //HttpContext.Current.Trace.Warn(string.Format("Failed to create the instance of the model binder"));
				//        }
				//    }
				//    else
				//    {
				//        //HttpContext.Current.Trace.Warn(string.Format("staticMapping type name {0} came back as null from Type.GetType; check the casing, assembly presence, assembly framework version, namespace", staticMapping.TypeName));
				//    }
				//}
				//else
				//{
				//    //HttpContext.Current.Trace.Write(string.Format("There isn't a staticMapping defined so checking the RazorDataTypeModelTypes cache..."));
				//}


				//if (RazorDataTypeModelTypes != null && RazorDataTypeModelTypes.Any(model => model.Key.Item1 == dataType) && dataType != Guid.Empty)
				//{
				//    var razorDataTypeModelDefinition = RazorDataTypeModelTypes.Where(model => model.Key.Item1 == dataType).OrderByDescending(model => model.Key.Item2).FirstOrDefault();
				//    if (!(razorDataTypeModelDefinition.Equals(default(KeyValuePair<System.Tuple<Guid, int>, Type>))))
				//    {
				//        Type dataTypeType = razorDataTypeModelDefinition.Value;
				//        object instance = null;
				//        if (TryCreateInstanceRazorDataTypeModel(dataType, dataTypeType, data.Value, out instance))
				//        {
				//            result = instance;
				//            return true;
				//        }
				//        else
				//        {
				//            //HttpContext.Current.Trace.Write("Failed");
				//            //HttpContext.Current.Trace.Warn(string.Format("Failed to create the instance of the model binder"));
				//        }
				//    }
				//    else
				//    {
				//        //HttpContext.Current.Trace.Write("Failed");
				//        //HttpContext.Current.Trace.Warn(string.Format("Could not get the dataTypeType for the RazorDataTypeModel"));
				//    }
				//}
				//else
				//{
				//    if (RazorDataTypeModelTypes == null)
				//    {
				//        //HttpContext.Current.Trace.Write(string.Format("RazorDataTypeModelTypes is null, probably an exception while building the cache, falling back to ConvertPropertyValueByDataType", dataType));
				//    }
				//    else
				//    {
				//        //HttpContext.Current.Trace.Write(string.Format("GUID {0} does not have a DataTypeModel, falling back to ConvertPropertyValueByDataType", dataType));
				//    }

				//}

				//convert the string value to a known type
				return ConvertPropertyValueByDataType(ref result, dataType);

			}

			//next, check if the alias is that of a child type
			// I'm pretty sure this means that if we have a 'Home' document type as the current type and it has children types that are for example called 'NewsPage' then
			// we can actually do this in the query: CurrentPage.NewsPages (plural) or CurrentPage.NewsPage (non-plural) and it will return the children of that type.

			var filteredTypeChildren = _backingItem.Children
				.Where(x => x.NodeTypeAlias.InvariantEquals(name) || x.NodeTypeAlias.MakePluralName().InvariantEquals(name))
				.ToArray();
			if (filteredTypeChildren.Any())
			{
				result = new DynamicDocumentList(filteredTypeChildren.Select(x => new DynamicDocument(x)));
				return true;
			}

			//lastly, we'll try to get the property from the document object's member properties using reflection

			var reflectedProperty = _backingItem.GetReflectedProperty(name);
			result = reflectedProperty != null ? reflectedProperty.Value : null;

			//if property access, type lookup and member invoke all failed
			//at this point, we're going to return null
			//instead, we return a DynamicNull - see comments in that file
			//this will let things like Model.ChildItem work and return nothing instead of crashing
			if (result == null)
			{
				//.Where explictly checks for this type
				//and will make it false
				//which means backwards equality (&& property != true) will pass
				//forwwards equality (&& property or && property == true) will fail
				result = new DynamicNull();
				return true;
			}
			return true;
		}

		//private bool TryCreateInstanceRazorDataTypeModel(Guid dataType, Type dataTypeType, string value, out object result)
		//{
		//    //HttpContext.Current.Trace.Write(string.Format("Found dataType {0} for GUID {1}", dataTypeType.FullName, dataType));
		//    IRazorDataTypeModel razorDataTypeModel = Activator.CreateInstance(dataTypeType, false) as IRazorDataTypeModel;
		//    //HttpContext.Current.Trace.Write(string.Format("Instantiating {0}...", dataTypeType.FullName));
		//    if (razorDataTypeModel != null)
		//    {
		//        //HttpContext.Current.Trace.Write("Success");
		//        object instance = null;
		//        //HttpContext.Current.Trace.Write("Calling Init on razorDataTypeModel");
		//        if (razorDataTypeModel.Init(_n.Id, value, out instance))
		//        {
		//            if (instance != null)
		//            {
		//                //HttpContext.Current.Trace.Write(string.Format("razorDataTypeModel successfully instantiated and returned a valid instance of type {0}", instance.GetType().FullName));
		//            }
		//            else
		//            {
		//                //HttpContext.Current.Trace.Warn("razorDataTypeModel successfully instantiated but returned null for instance");
		//            }
		//            result = instance;
		//            return true;
		//        }
		//        else
		//        {
		//            if (instance != null)
		//            {
		//                //HttpContext.Current.Trace.Write(string.Format("razorDataTypeModel returned false but returned a valid instance of type {0}", instance.GetType().FullName));
		//            }
		//            else
		//            {
		//                //HttpContext.Current.Trace.Warn("razorDataTypeModel successfully instantiated but returned null for instance");
		//            }
		//        }
		//    }
		//    else
		//    {
		//        //HttpContext.Current.Trace.Write("Failed");
		//        //HttpContext.Current.Trace.Warn(string.Format("DataTypeModel {0} failed to instantiate, perhaps it is lacking a parameterless constructor or doesn't implement IRazorDataTypeModel?", dataTypeType.FullName));
		//    }
		//    result = null;
		//    return false;
		//}
		private bool ConvertPropertyValueByDataType(ref object result, Guid dataType)
		{

			//First lets check all registered converters:
			// TODO: Look up new converters!

			//the resulting property is a string, but to support some of the nice linq stuff in .Where
			//we should really check some more types
			string sResult = string.Format("{0}", result).Trim();

			

			//this will eat csv strings, so only do it if the decimal also includes a decimal seperator (according to the current culture)
			if (sResult.Contains(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
			{
				//decimal
				decimal dResult = 0;
				if (decimal.TryParse(sResult, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.CurrentCulture, out dResult))
				{
					result = dResult;
					return true;
				}
			}
		


			if (string.Equals("true", sResult, StringComparison.CurrentCultureIgnoreCase))
			{
				result = true;
				return true;
			}
			if (string.Equals("false", sResult, StringComparison.CurrentCultureIgnoreCase))
			{
				result = false;
				return true;
			}

			if (result != null)
			{
				//a really rough check to see if this may be valid xml
				if (sResult.StartsWith("<") && sResult.EndsWith(">") && sResult.Contains("/"))
				{
					try
					{
						XElement e = XElement.Parse(DynamicXml.StripDashesInElementOrAttributeNames(sResult), LoadOptions.None);
						if (e != null)
						{
							//check that the document element is not one of the disallowed elements
							//allows RTE to still return as html if it's valid xhtml
							string documentElement = e.Name.LocalName;
							if (!UmbracoSettings.NotDynamicXmlDocumentElements.Any(tag =>
								string.Equals(tag, documentElement, StringComparison.CurrentCultureIgnoreCase)))
							{
								result = new DynamicXml(e);
								return true;
							}
							else
							{
								//we will just return this as a string
								return true;
							}
						}
					}
					catch (Exception)
					{
						//we will just return this as a string
						return true;
					}

				}
			}

			return true;
		}

		//public DynamicNode Media(string propertyAlias)
		//{
		//    if (_n != null)
		//    {
		//        IProperty prop = _n.GetProperty(propertyAlias);
		//        if (prop != null)
		//        {
		//            int mediaNodeId;
		//            if (int.TryParse(prop.Value, out mediaNodeId))
		//            {
		//                return _razorLibrary.Value.MediaById(mediaNodeId);
		//            }
		//        }
		//        return null;
		//    }
		//    return null;
		//}
		//public bool IsProtected
		//{
		//    get
		//    {
		//        if (_n != null)
		//        {
		//            return umbraco.library.IsProtected(_n.Id, _n.Path);
		//        }
		//        return false;
		//    }
		//}
		//public bool HasAccess
		//{
		//    get
		//    {
		//        if (_n != null)
		//        {
		//            return umbraco.library.HasAccess(_n.Id, _n.Path);
		//        }
		//        return true;
		//    }
		//}

		//public string Media(string propertyAlias, string mediaPropertyAlias)
		//{
		//    if (_n != null)
		//    {
		//        IProperty prop = _n.GetProperty(propertyAlias);
		//        if (prop == null && propertyAlias.Substring(0, 1).ToUpper() == propertyAlias.Substring(0, 1))
		//        {
		//            prop = _n.GetProperty(propertyAlias.Substring(0, 1).ToLower() + propertyAlias.Substring((1)));
		//        }
		//        if (prop != null)
		//        {
		//            int mediaNodeId;
		//            if (int.TryParse(prop.Value, out mediaNodeId))
		//            {
		//                umbraco.cms.businesslogic.media.Media media = new cms.businesslogic.media.Media(mediaNodeId);
		//                if (media != null)
		//                {
		//                    Property mprop = media.getProperty(mediaPropertyAlias);
		//                    // check for nicer support of Pascal Casing EVEN if alias is camelCasing:
		//                    if (prop == null && mediaPropertyAlias.Substring(0, 1).ToUpper() == mediaPropertyAlias.Substring(0, 1))
		//                    {
		//                        mprop = media.getProperty(mediaPropertyAlias.Substring(0, 1).ToLower() + mediaPropertyAlias.Substring((1)));
		//                    }
		//                    if (mprop != null)
		//                    {
		//                        return string.Format("{0}", mprop.Value);
		//                    }
		//                }
		//            }
		//        }
		//    }
		//    return null;
		//}

		public DynamicDocument AncestorOrSelf()
		{
			//TODO: Why is this query like this??
			return AncestorOrSelf(node => node.Level == 1);
		}
		public DynamicDocument AncestorOrSelf(int level)
		{
			return AncestorOrSelf(node => node.Level == level);
		}
		public DynamicDocument AncestorOrSelf(string nodeTypeAlias)
		{
			return AncestorOrSelf(node => node.NodeTypeAlias == nodeTypeAlias);
		}
		public DynamicDocument AncestorOrSelf(Func<DynamicDocument, bool> func)
		{
			var node = this;
			while (node != null)
			{
				if (func(node)) return node;
				DynamicDocument parent = node.Parent;
				if (parent != null)
				{
					if (this != parent)
					{
						node = parent;
					}
					else
					{
						return node;
					}
				}
				else
				{
					return null;
				}
			}
			return node;
		}
		public DynamicDocumentList AncestorsOrSelf(Func<DynamicDocument, bool> func)
		{
			var ancestorList = new List<DynamicDocument>();
			var node = this;
			ancestorList.Add(node);
			while (node != null)
			{
				if (node.Level == 1) break;
				DynamicDocument parent = node.Parent;
				if (parent != null)
				{
					if (this != parent)
					{
						node = parent;
						if (func(node))
						{
							ancestorList.Add(node);
						}
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}
			ancestorList.Reverse();
			return new DynamicDocumentList(ancestorList);
		}
		public DynamicDocumentList AncestorsOrSelf()
		{
			return AncestorsOrSelf(n => true);
		}
		public DynamicDocumentList AncestorsOrSelf(string nodeTypeAlias)
		{
			return AncestorsOrSelf(n => n.NodeTypeAlias == nodeTypeAlias);
		}
		public DynamicDocumentList AncestorsOrSelf(int level)
		{
			return AncestorsOrSelf(n => n.Level <= level);
		}
		public DynamicDocumentList Descendants(string nodeTypeAlias)
		{
			return Descendants(p => p.NodeTypeAlias == nodeTypeAlias);
		}
		public DynamicDocumentList Descendants(int level)
		{
			return Descendants(p => p.Level >= level);
		}
		public DynamicDocumentList Descendants()
		{
			return Descendants(n => true);
		}
		public DynamicDocumentList Descendants(Func<DynamicBackingItem, bool> func)
		{
			var flattenedNodes = this._backingItem.Children.Map(func, (DynamicBackingItem n) => n.Children);
			return new DynamicDocumentList(flattenedNodes.ToList().ConvertAll(dynamicBackingItem => new DynamicDocument(dynamicBackingItem)));
		}
		public DynamicDocumentList DescendantsOrSelf(int level)
		{
			return DescendantsOrSelf(p => p.Level >= level);
		}
		public DynamicDocumentList DescendantsOrSelf(string nodeTypeAlias)
		{
			return DescendantsOrSelf(p => p.NodeTypeAlias == nodeTypeAlias);
		}
		public DynamicDocumentList DescendantsOrSelf()
		{
			return DescendantsOrSelf(p => true);
		}
		public DynamicDocumentList DescendantsOrSelf(Func<DynamicBackingItem, bool> func)
		{
			if (this._backingItem != null)
			{
				var thisNode = new List<DynamicBackingItem>();
				if (func(this._backingItem))
				{
					thisNode.Add(this._backingItem);
				}
				var flattenedNodes = this._backingItem.Children.Map(func, (DynamicBackingItem n) => n.Children);
				return new DynamicDocumentList(thisNode.Concat(flattenedNodes).ToList().ConvertAll(dynamicBackingItem => new DynamicDocument(dynamicBackingItem)));
			}
			return new DynamicDocumentList(Enumerable.Empty<IDocument>());
		}
		public DynamicDocumentList Ancestors(int level)
		{
			return Ancestors(n => n.Level <= level);
		}
		public DynamicDocumentList Ancestors(string nodeTypeAlias)
		{
			return Ancestors(n => n.NodeTypeAlias == nodeTypeAlias);
		}
		public DynamicDocumentList Ancestors()
		{
			return Ancestors(n => true);
		}
		public DynamicDocumentList Ancestors(Func<DynamicDocument, bool> func)
		{
			var ancestorList = new List<DynamicDocument>();
			var node = this;
			while (node != null)
			{
				if (node.Level == 1) break;
				DynamicDocument parent = node.Parent;
				if (parent != null)
				{
					if (this != parent)
					{
						node = parent;
						if (func(node))
						{
							ancestorList.Add(node);
						}
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}
			ancestorList.Reverse();
			return new DynamicDocumentList(ancestorList);
		}
		public DynamicDocument Parent
		{
			get
			{
				if (_backingItem.Parent != null)
				{
					return new DynamicDocument(_backingItem.Parent);
				}
				if (_backingItem != null && _backingItem.Id == 0)
				{
					return this;
				}
				return null;
			}
		}

		//private readonly Lazy<RazorLibraryCore> _razorLibrary = new Lazy<RazorLibraryCore>(() => new RazorLibraryCore(null));

		public int TemplateId
		{
			get { return _backingItem.TemplateId; }
		}

		public int SortOrder
		{
			get { return _backingItem.SortOrder; }
		}

		public string Name
		{
			get { return _backingItem.Name; }
		}
		public bool Visible
		{
			get
			{

				var umbracoNaviHide = _backingItem.GetUserProperty("umbracoNaviHide");
				if (umbracoNaviHide != null)
				{
					return umbracoNaviHide.Value != "1";
				}
				return true;
			}
		}
		//public string Url
		//{
		//    get { if (_n == null) return null; return _n.Url; }
		//}

		public string UrlName
		{
			get { return _backingItem.UrlName; }
		}

		public string NodeTypeAlias
		{
			get { return _backingItem.NodeTypeAlias; }
		}

		public string WriterName
		{
			get { return _backingItem.WriterName; }
		}

		public string CreatorName
		{
			get { return _backingItem.CreatorName; }
		}

		public int WriterId
		{
			get { return _backingItem.WriterId; }
		}

		public int CreatorId
		{
			get { return _backingItem.CreatorId; }
		}

		public string Path
		{
			get { return _backingItem.Path; }
		}

		public DateTime CreateDate
		{
			get { return _backingItem.CreateDate; }
		}
		public int Id
		{
			get { return _backingItem.Id; }
		}

		public DateTime UpdateDate
		{
			get { return _backingItem.UpdateDate; }
		}

		public Guid Version
		{
			get { return _backingItem.Version; }
		}

		//public string NiceUrl
		//{
		//    get { if (_n == null) return null; return _n.NiceUrl; }
		//}

		public int Level
		{
			get { return _backingItem.Level; }
		}

		public IEnumerable<IDocumentProperty> Properties
		{
			get { return _backingItem.Properties; }
		}

		//public IEnumerable<DynamicBackingItem> Children
		//{
		//    get { if (_n == null) return null; return _n.Children; }
		//}

		public IEnumerable<DynamicDocument> Children
		{
			get
			{
				if (_cachedChildren == null)
				{
					var children = _backingItem.Children;
					//testing
					if (!children.Any() && _backingItem.Id == 0)
					{
						_cachedChildren = new DynamicDocumentList(new List<DynamicDocument> { new DynamicDocument(this._backingItem) });
					}
					else
					{
						_cachedChildren = new DynamicDocumentList(_backingItem.Children.Select(x => new DynamicDocument(x)));
					}
				}
				return _cachedChildren;
			}
		}

		#region GetProperty methods which can be used with the dynamic object

		public IDocumentProperty GetProperty(string alias)
		{
			return GetProperty(alias, false);
		}
		public IDocumentProperty GetProperty(string alias, bool recursive)
		{
			return alias.StartsWith("@")
				? _backingItem.GetReflectedProperty(alias.TrimStart('@'))
				: _backingItem.GetUserProperty(alias, recursive);
		}
		public string GetPropertyValue(string alias)
		{
			return GetPropertyValue(alias, false);
		}
		public string GetPropertyValue(string alias, string fallback)
		{
			var prop = GetPropertyValue(alias);
			return !prop.IsNullOrWhiteSpace() ? prop : fallback;
		}
		public string GetPropertyValue(string alias, bool recursive)
		{
			var p = alias.StartsWith("@")
					? _backingItem.GetReflectedProperty(alias.TrimStart('@'))
					: _backingItem.GetUserProperty(alias, recursive);
			return p == null ? null : p.ValueAsString;
		}
		public string GetPropertyValue(string alias, bool recursive, string fallback)
		{
			var prop = GetPropertyValue(alias, recursive);
			return !prop.IsNullOrWhiteSpace() ? prop : fallback;
		}

		#endregion

		//public System.Data.DataTable ChildrenAsTable()
		//{
		//    if (_n == null) return null;
		//    return _n.ChildrenAsTable();
		//}

		//public System.Data.DataTable ChildrenAsTable(string nodeTypeAliasFilter)
		//{
		//    if (_n == null) return null;
		//    return _n.ChildrenAsTable(nodeTypeAliasFilter);
		//}
		public bool IsNull(string alias, bool recursive)
		{
			var prop = _backingItem.GetUserProperty(alias, recursive);
			if (prop == null) return true;
			return ((PropertyResult)prop).HasValue();
		}
		public bool IsNull(string alias)
		{
			return IsNull(alias, false);
		}
		public bool HasValue(string alias)
		{
			return HasValue(alias, false);
		}
		public bool HasValue(string alias, bool recursive)
		{
			var prop = _backingItem.GetUserProperty(alias, recursive);
			//var prop = GetProperty(alias, recursive);
			if (prop == null) return false;
			return prop.HasValue();
		}
		public IHtmlString HasValue(string alias, string valueIfTrue, string valueIfFalse)
		{
			return HasValue(alias, false) ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
		}
		public IHtmlString HasValue(string alias, bool recursive, string valueIfTrue, string valueIfFalse)
		{
			return HasValue(alias, recursive) ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
		}
		public IHtmlString HasValue(string alias, string valueIfTrue)
		{
			return HasValue(alias, false) ? new HtmlString(valueIfTrue) : new HtmlString(string.Empty);
		}
		public IHtmlString HasValue(string alias, bool recursive, string valueIfTrue)
		{
			return HasValue(alias, recursive) ? new HtmlString(valueIfTrue) : new HtmlString(string.Empty);
		}
		public int Position()
		{
			return this.Index();
		}
		public int Index()
		{
			if (this.OwnerList == null && this.Parent != null)
			{
				//var list = this.Parent.Children.Select(n => new DynamicNode(n));
				var list = this.Parent.Children;
				this.OwnerList = new DynamicDocumentList(list);
			}
			if (this.OwnerList != null)
			{
				List<DynamicDocument> container = this.OwnerList.Items.ToList();
				int currentIndex = container.FindIndex(n => n.Id == this.Id);
				if (currentIndex != -1)
				{
					return currentIndex;
				}
				else
				{
					throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicNodeList but could not retrieve the index for it's position in the list", this.Id));
				}
			}
			else
			{
				throw new ArgumentNullException(string.Format("Node {0} has been orphaned and doesn't belong to a DynamicNodeList", this.Id));
			}
		}
		public bool IsFirst()
		{
			return IsHelper(n => n.Index() == 0);
		}
		public HtmlString IsFirst(string valueIfTrue)
		{
			return IsHelper(n => n.Index() == 0, valueIfTrue);
		}
		public HtmlString IsFirst(string valueIfTrue, string valueIfFalse)
		{
			return IsHelper(n => n.Index() == 0, valueIfTrue, valueIfFalse);
		}
		public bool IsNotFirst()
		{
			return !IsHelper(n => n.Index() == 0);
		}
		public HtmlString IsNotFirst(string valueIfTrue)
		{
			return IsHelper(n => n.Index() != 0, valueIfTrue);
		}
		public HtmlString IsNotFirst(string valueIfTrue, string valueIfFalse)
		{
			return IsHelper(n => n.Index() != 0, valueIfTrue, valueIfFalse);
		}
		public bool IsPosition(int index)
		{
			if (this.OwnerList == null)
			{
				return false;
			}
			return IsHelper(n => n.Index() == index);
		}
		public HtmlString IsPosition(int index, string valueIfTrue)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(string.Empty);
			}
			return IsHelper(n => n.Index() == index, valueIfTrue);
		}
		public HtmlString IsPosition(int index, string valueIfTrue, string valueIfFalse)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(valueIfFalse);
			}
			return IsHelper(n => n.Index() == index, valueIfTrue, valueIfFalse);
		}
		public bool IsModZero(int modulus)
		{
			if (this.OwnerList == null)
			{
				return false;
			}
			return IsHelper(n => n.Index() % modulus == 0);
		}
		public HtmlString IsModZero(int modulus, string valueIfTrue)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(string.Empty);
			}
			return IsHelper(n => n.Index() % modulus == 0, valueIfTrue);
		}
		public HtmlString IsModZero(int modulus, string valueIfTrue, string valueIfFalse)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(valueIfFalse);
			}
			return IsHelper(n => n.Index() % modulus == 0, valueIfTrue, valueIfFalse);
		}

		public bool IsNotModZero(int modulus)
		{
			if (this.OwnerList == null)
			{
				return false;
			}
			return IsHelper(n => n.Index() % modulus != 0);
		}
		public HtmlString IsNotModZero(int modulus, string valueIfTrue)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(string.Empty);
			}
			return IsHelper(n => n.Index() % modulus != 0, valueIfTrue);
		}
		public HtmlString IsNotModZero(int modulus, string valueIfTrue, string valueIfFalse)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(valueIfFalse);
			}
			return IsHelper(n => n.Index() % modulus != 0, valueIfTrue, valueIfFalse);
		}
		public bool IsNotPosition(int index)
		{
			if (this.OwnerList == null)
			{
				return false;
			}
			return !IsHelper(n => n.Index() == index);
		}
		public HtmlString IsNotPosition(int index, string valueIfTrue)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(string.Empty);
			}
			return IsHelper(n => n.Index() != index, valueIfTrue);
		}
		public HtmlString IsNotPosition(int index, string valueIfTrue, string valueIfFalse)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(valueIfFalse);
			}
			return IsHelper(n => n.Index() != index, valueIfTrue, valueIfFalse);
		}
		public bool IsLast()
		{
			if (this.OwnerList == null)
			{
				return false;
			}
			int count = this.OwnerList.Count();
			return IsHelper(n => n.Index() == count - 1);
		}
		public HtmlString IsLast(string valueIfTrue)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(string.Empty);
			}
			int count = this.OwnerList.Count();
			return IsHelper(n => n.Index() == count - 1, valueIfTrue);
		}
		public HtmlString IsLast(string valueIfTrue, string valueIfFalse)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(valueIfFalse);
			}
			int count = this.OwnerList.Count();
			return IsHelper(n => n.Index() == count - 1, valueIfTrue, valueIfFalse);
		}
		public bool IsNotLast()
		{
			if (this.OwnerList == null)
			{
				return false;
			}
			int count = this.OwnerList.Count();
			return !IsHelper(n => n.Index() == count - 1);
		}
		public HtmlString IsNotLast(string valueIfTrue)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(string.Empty);
			}
			int count = this.OwnerList.Count();
			return IsHelper(n => n.Index() != count - 1, valueIfTrue);
		}
		public HtmlString IsNotLast(string valueIfTrue, string valueIfFalse)
		{
			if (this.OwnerList == null)
			{
				return new HtmlString(valueIfFalse);
			}
			int count = this.OwnerList.Count();
			return IsHelper(n => n.Index() != count - 1, valueIfTrue, valueIfFalse);
		}
		public bool IsEven()
		{
			return IsHelper(n => n.Index() % 2 == 0);
		}
		public HtmlString IsEven(string valueIfTrue)
		{
			return IsHelper(n => n.Index() % 2 == 0, valueIfTrue);
		}
		public HtmlString IsEven(string valueIfTrue, string valueIfFalse)
		{
			return IsHelper(n => n.Index() % 2 == 0, valueIfTrue, valueIfFalse);
		}
		public bool IsOdd()
		{
			return IsHelper(n => n.Index() % 2 == 1);
		}
		public HtmlString IsOdd(string valueIfTrue)
		{
			return IsHelper(n => n.Index() % 2 == 1, valueIfTrue);
		}
		public HtmlString IsOdd(string valueIfTrue, string valueIfFalse)
		{
			return IsHelper(n => n.Index() % 2 == 1, valueIfTrue, valueIfFalse);
		}
		public bool IsEqual(DynamicDocument other)
		{
			return IsHelper(n => n.Id == other.Id);
		}
		public HtmlString IsEqual(DynamicDocument other, string valueIfTrue)
		{
			return IsHelper(n => n.Id == other.Id, valueIfTrue);
		}
		public HtmlString IsEqual(DynamicDocument other, string valueIfTrue, string valueIfFalse)
		{
			return IsHelper(n => n.Id == other.Id, valueIfTrue, valueIfFalse);
		}
		public bool IsNotEqual(DynamicDocument other)
		{
			return IsHelper(n => n.Id != other.Id);
		}
		public HtmlString IsNotEqual(DynamicDocument other, string valueIfTrue)
		{
			return IsHelper(n => n.Id != other.Id, valueIfTrue);
		}
		public HtmlString IsNotEqual(DynamicDocument other, string valueIfTrue, string valueIfFalse)
		{
			return IsHelper(n => n.Id != other.Id, valueIfTrue, valueIfFalse);
		}
		public bool IsDescendant(DynamicDocument other)
		{
			var ancestors = this.Ancestors();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null);
		}
		public HtmlString IsDescendant(DynamicDocument other, string valueIfTrue)
		{
			var ancestors = this.Ancestors();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue);
		}
		public HtmlString IsDescendant(DynamicDocument other, string valueIfTrue, string valueIfFalse)
		{
			var ancestors = this.Ancestors();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public bool IsDescendantOrSelf(DynamicDocument other)
		{
			var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null);
		}
		public HtmlString IsDescendantOrSelf(DynamicDocument other, string valueIfTrue)
		{
			var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue);
		}
		public HtmlString IsDescendantOrSelf(DynamicDocument other, string valueIfTrue, string valueIfFalse)
		{
			var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public bool IsAncestor(DynamicDocument other)
		{
			var descendants = this.Descendants();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null);
		}
		public HtmlString IsAncestor(DynamicDocument other, string valueIfTrue)
		{
			var descendants = this.Descendants();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue);
		}
		public HtmlString IsAncestor(DynamicDocument other, string valueIfTrue, string valueIfFalse)
		{
			var descendants = this.Descendants();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public bool IsAncestorOrSelf(DynamicDocument other)
		{
			var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null);
		}
		public HtmlString IsAncestorOrSelf(DynamicDocument other, string valueIfTrue)
		{
			var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue);
		}
		public HtmlString IsAncestorOrSelf(DynamicDocument other, string valueIfTrue, string valueIfFalse)
		{
			var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue, valueIfFalse);
		}
		public bool IsHelper(Func<DynamicDocument, bool> test)
		{
			return test(this);
		}
		public HtmlString IsHelper(Func<DynamicDocument, bool> test, string valueIfTrue)
		{
			return IsHelper(test, valueIfTrue, string.Empty);
		}
		public HtmlString IsHelper(Func<DynamicDocument, bool> test, string valueIfTrue, string valueIfFalse)
		{
			return test(this) ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
		}
		public HtmlString Where(string predicate, string valueIfTrue)
		{
			return Where(predicate, valueIfTrue, string.Empty);
		}
		public HtmlString Where(string predicate, string valueIfTrue, string valueIfFalse)
		{
			if (Where(predicate))
			{
				return new HtmlString(valueIfTrue);
			}
			return new HtmlString(valueIfFalse);
		}
		public bool Where(string predicate)
		{
			//Totally gonna cheat here
			var dynamicNodeList = new DynamicDocumentList();
			dynamicNodeList.Add(this);
			var filtered = dynamicNodeList.Where<DynamicDocument>(predicate);
			if (Queryable.Count(filtered) == 1)
			{
				//this node matches the predicate
				return true;
			}
			return false;
		}
	}
}