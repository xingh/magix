﻿<%@ Assembly 
    Name="Magix.Brix.Viewports" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.Brix.Viewports.SingleContainer" %>

<mux:AjaxWait 
    runat="server" 
    CssClass="ajax-wait"
    Delay="500"
    MaxOpacity="0.8"
    id="waiter">
    <p>Please wait while Marvin is thinking ...</p>
    <mux:Image
        runat="server"
        id="ajaxWait"
        ImageUrl="media/images/ajax.gif" 
        AlternateText="Marvin's brain ..." />
</mux:AjaxWait>

<mux:Panel
    runat="server"
    id="wrp"
    CssClass="container main">
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="content1" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="content2" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="content3" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="content4" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="content5" />
    <mux:Panel
        runat="server"
        CssClass="span-24 last childContainer"
        Visible="false"
        id="pnlAll" />
    <mux:Window 
        runat="server" 
        CssClass="mux-shaded mux-rounded mux-window message last msg-center"
        Caption="Message from Marvin ..."
        style="position:fixed;display:none;top:54px;left:190px;"
        Closable="false"
        id="message">
        <Content>
            <mux:Label 
                runat="server" 
                id="msgLbl" />
        </Content>
    </mux:Window>
</mux:Panel>
