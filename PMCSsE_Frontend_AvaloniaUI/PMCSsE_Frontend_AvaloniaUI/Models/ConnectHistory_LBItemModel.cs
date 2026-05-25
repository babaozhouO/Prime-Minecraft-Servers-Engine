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

using PMCSsE_Frontend_AvaloniaUI.Modules;

namespace PMCSsE_Frontend_AvaloniaUI.Models
{
    public class ConnectHistory_LBItemModel
    {
        internal ConnectHistory_LBItemModel(NativeServerHistory nsh)
        {
            MyNativeServerHistory = nsh;
        }
        internal NativeServerHistory MyNativeServerHistory { get; set; }
        internal string DisplayText => MyNativeServerHistory.IP + ':' + MyNativeServerHistory.Port;
    }
}
