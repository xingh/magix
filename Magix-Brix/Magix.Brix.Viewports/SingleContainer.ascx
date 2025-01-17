﻿<%@ Assembly 
    Name="Magix.Brix.Viewports" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.Brix.Viewports.SingleContainer" %>

<mux:Audio
    runat="server"
    id="audio" />

<mux:AjaxWait 
    runat="server" 
    CssClass="mux-ajax-wait"
    Delay="500"
    MaxOpacity="0.8"
    id="waiter">
    <p>Please wait ...</p>
    <mux:Image
        runat="server"
        id="ajaxWait"
        ImageUrl="media/images/ajax.gif" 
        AlternateText="Marvin's brain ..." />
</mux:AjaxWait>

<mux:Timer
    runat="server"
    id="timer"
    OnTick="timer_Tick"
    Enabled="false" />

<mux:Panel
    runat="server"
    CssClass="mux-shaded mux-rounded mux-debug-panel"
    Visible="false"
    id="debug" />

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
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="content6" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="content7" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="floater" />
    <mux:Panel
        runat="server"
        CssClass="span-24 last mux-child-container"
        Visible="false"
        id="pnlAll" />
    <mux:Window 
        runat="server" 
        CssClass="mux-shaded mux-rounded mux-window mux-message last"
        Caption="Message from Marvin ..."
        style="display:none;"
        Draggable="false"
        Closable="false"
        id="message">
        <Content>
            <mux:Label 
                runat="server" 
                id="msgLbl" />
        </Content>
    </mux:Window>

    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        CssClass="full-screen"
        style="display:none;"
        id="fullScreen" />

</mux:Panel>
