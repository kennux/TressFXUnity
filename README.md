Unity TressFX Portation
==============

This project is a portation of AMD's TressFX hair simulation aiming for fully implementing realtime hair rendering and simulation into Unity3D.
AMD's TressFX Sample can get found here: http://developer.amd.com/tools-and-sdks/graphics-development/amd-radeon-sdk/

Simulation
==============

The simulation is based on a DX11 ComputeShader which does all the Simulation of the hair on the GPU.

Features
==============

* Fully implemented TressFX 2.2 Simulation
* TressFX-Equivalent rendering implementation
* Unity editor integration

Licenses
==============

AMD Files (HairSimulate.compute, the example hairs and character model and some parts of the rendering shaders)
--------------
Copyright 2014 ADVANCED MICRO DEVICES, INC.  All Rights Reserved.

AMD is granting you permission to use this software and documentation (if
any) (collectively, the “Materials”) pursuant to the terms and conditions
of the Software License Agreement included with the Materials.  If you do
not have a copy of the Software License Agreement, contact your AMD
representative for a copy.
You agree that you will not reverse engineer or decompile the Materials,
in whole or in part, except as allowed by applicable law.

WARRANTY DISCLAIMER: THE SOFTWARE IS PROVIDED "AS IS" WITHOUT WARRANTY OF
ANY KIND.  AMD DISCLAIMS ALL WARRANTIES, EXPRESS, IMPLIED, OR STATUTORY,
INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE, TITLE, NON-INFRINGEMENT, THAT THE SOFTWARE
WILL RUN UNINTERRUPTED OR ERROR-FREE OR WARRANTIES ARISING FROM CUSTOM OF
TRADE OR COURSE OF USAGE.  THE ENTIRE RISK ASSOCIATED WITH THE USE OF THE
SOFTWARE IS ASSUMED BY YOU.
Some jurisdictions do not allow the exclusion of implied warranties, so
the above exclusion may not apply to You. 

LIMITATION OF LIABILITY AND INDEMNIFICATION:  AMD AND ITS LICENSORS WILL
NOT, UNDER ANY CIRCUMSTANCES BE LIABLE TO YOU FOR ANY PUNITIVE, DIRECT,
INCIDENTAL, INDIRECT, SPECIAL OR CONSEQUENTIAL DAMAGES ARISING FROM USE OF
THE SOFTWARE OR THIS AGREEMENT EVEN IF AMD AND ITS LICENSORS HAVE BEEN
ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.  
In no event shall AMD's total liability to You for all damages, losses,
and causes of action (whether in contract, tort (including negligence) or
otherwise) exceed the amount of $100 USD.  You agree to defend, indemnify
and hold harmless AMD and its licensors, and any of their directors,
officers, employees, affiliates or agents from and against any and all
loss, damage, liability and other expenses (including reasonable attorneys'
fees), resulting from Your use of the Software or violation of the terms and
conditions of this Agreement.  

U.S. GOVERNMENT RESTRICTED RIGHTS: The Materials are provided with "RESTRICTED
RIGHTS." Use, duplication, or disclosure by the Government is subject to the
restrictions as set forth in FAR 52.227-14 and DFAR252.227-7013, et seq., or
its successor.  Use of the Materials by the Government constitutes
acknowledgement of AMD's proprietary rights in them.

EXPORT RESTRICTIONS: The Materials may be subject to export restrictions as
stated in the Software License Agreement.

Unity Implementation (CSharp files and some parts of the HairShader)
--------------

Copyright (c) 2014 Kenneth Ellersdorfer
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.