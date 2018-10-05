/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * Licensed under the MIT License (the "License"); 
 * you may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, 
 * software distributed under the License is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. 
 * See the License for the specific language governing permissions and limitations under the License.
*/



using UnityEngine;
using UnityEngine.EventSystems;

namespace SGF.Unity.UI.UILib.Control
{
	public class UIControl:MonoBehaviour
	{
	    public bool AutoBindUIElement = false;

        protected virtual void Awake()
		{
		    if (AutoBindUIElement)
		    {
		        UIElementBinder.BindAllUIElement(this);
		    }
		}

		public virtual void SetData(object data)
		{ 
		}

		 

		public void SetVisible(bool value)
		{
			this.gameObject.SetActive(value);
		}

		public static void SetVisible(UIBehaviour ui, bool value)
		{
			if (ui != null)
			{
				ui.gameObject.SetActive(value);
			}
		}

		public bool IsVisible { get { return this.gameObject.activeSelf;} }

		public static bool GetVisible(UIBehaviour ui)
		{
			return ui.gameObject.activeSelf;
		}

	}
}
