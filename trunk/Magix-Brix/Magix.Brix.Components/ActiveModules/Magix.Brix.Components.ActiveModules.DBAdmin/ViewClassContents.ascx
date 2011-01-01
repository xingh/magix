﻿<%@ Assembly 
    Name="Magix.Brix.Components.ActiveModules.DBAdmin" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.Brix.Components.ActiveModules.DBAdmin.ViewClassContents" %>

<mux:Panel
    runat="server"
    style="font-size:small;"
    id="pnl">
</mux:Panel>
<mux:Button
    runat="server"
    OnClick="PreviousItems"
    id="previous" />

<mux:Button
    runat="server"
    OnClick="NextItems"
    id="next" />

<mux:Button
    runat="server"
    Text="Create new item..."
    OnClick="CreateItem"
    id="create" />


