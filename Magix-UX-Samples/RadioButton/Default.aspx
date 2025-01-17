﻿<%@ Page 
    Language="C#" 
    AutoEventWireup="true"
    CodeFile="Default.aspx.cs" 
    Inherits="RadioButtonSample" %>

<!DOCTYPE html>

<html>
    <head runat="server">
        <title>Untitled Page</title>
        <link rel="stylesheet" href="../media/blueprint/screen.css" type="text/css" media="screen, projection" />
        <link rel="stylesheet" href="../media/blueprint/print.css" type="text/css" media="print" />
        <!--[if lt IE 8]>
        <link rel="stylesheet" href="../media/blueprint/ie.css" type="text/css" media="screen, projection" />
        <![endif]-->

        <link href="../media/skins/default/default.css" rel="stylesheet" type="text/css" />
    </head>
    <body>
        <form id="form1" runat="server">
            <div class="container">
                <div class="span-10 last prepend-top">
                    <p>
                        <mux:Label 
                            runat="server" 
                            Text="Select one thing"
                            ID="lbl" />
                    </p>
                    <p class="push-1">
                        <mux:RadioButton 
                            runat="server" 
                            GroupName="rdos"
                            id="rdo1"
                            Info="Milk" 
                            OnCheckedChanged="chk_CheckedChanged" />
                        <mux:Label 
                            runat="server" 
                            ID="Label1" 
                            Tag="label"
                            For="rdo1"
                            Text="Milk" />
                    </p>
                    <p class="push-2 last">
                        <mux:RadioButton 
                            runat="server" 
                            GroupName="rdos"
                            id="rdo2" 
                            Info="Honey" 
                            OnCheckedChanged="chk_CheckedChanged" />
                        <mux:Label 
                            runat="server" 
                            ID="Label2" 
                            Tag="label"
                            For="rdo2"
                            Text="Honey" />
                    </p>
                </div>
            </div>
        </form>
    </body>
</html>
