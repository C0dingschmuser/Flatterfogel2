using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using CodeStage.AntiCheat.ObscuredTypes;

public partial class SROptions
{

    // Default Value for property
    private ObscuredBool _ignorePipes = false;
    private ObscuredBool _ignoreMinus = false;
    private ObscuredBool _ignoreGround = false;

    // Options will be grouped by category
    [Category("Cheats")]
    public bool IgnorePipes
    {
        get { return _ignorePipes; }
        set { _ignorePipes = value; }
    }
    [Category("Cheats")]
    public bool IgnoreMinus
    {
        get { return _ignoreMinus; }
        set { _ignoreMinus = value; }
    }
    [Category("Cheats")]
    public bool IgnoreGround
    {
        get { return _ignoreGround; }
        set { _ignoreGround = value; }
    }

}
