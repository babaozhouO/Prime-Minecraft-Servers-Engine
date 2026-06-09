/*Copyright 2025 【Babao Zhou (Legal Name: RenJie Zhou) <Contact: 1749861851@qq.com>】

   Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.*/
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PMCSsE_Frontend_AvaloniaUI.ViewModels;
using System;
using System.Diagnostics.CodeAnalysis;

namespace PMCSsE_Frontend_AvaloniaUI
{
    /// <summary>
    /// Given a view model, returns the corresponding view if possible.
    /// </summary>
    [RequiresUnreferencedCode(
        "Default implementation of ViewLocator involves reflection which may be trimmed away.",
        Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
    public class ViewLocator : IDataTemplate
    {
        /// <summary>
        /// 根据 ViewModel 实例查找对应的 View 并构建控件。
        /// 通过将 ViewModel 类名中的 "ViewModel" 替换为 "View" 来定位 View 类型。
        /// </summary>
        /// <param name="param">ViewModel 实例。</param>
        /// <returns>对应的 View 控件，若未找到则返回显示错误信息的 TextBlock。</returns>
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        /// <summary>
        /// 判断指定数据对象是否为 ViewModelBase 类型，以决定是否使用此模板。
        /// </summary>
        /// <param name="data">要检查的数据对象。</param>
        /// <returns>若数据对象是 ViewModelBase 则返回 true。</returns>
        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}