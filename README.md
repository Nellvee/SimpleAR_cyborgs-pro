# SimpleAR_cyborgs-pro

Simple AR project where we can download 1 model from web, then place it on every horizontal surface in AR.

The project took 7 hours and 54 minutes to complete.

Unity: 6.3 LTS (6000.3.0f1)
AR Packages: AR Foundation
Other Packages: Quick Outline, UnityGLTF

How to use the project:
1. git clone https://github.com/Nellvee/SimpleAR_cyborgs-pro.git
2. Launch Unity.
3. Click Play or Build and Run.
Or:
1. Upload the .apk to your device.
2. Install the .apk.
Or:
1. Connect your device to your PC (enable debugging on the device).
2. Find adb.exe
3. Enter adb install <path_to_apk_file> in the terminal.
4. Run the application.

How to use the application:
1. Specify the URL from which you want to download the .glb model.
2. Download the model by clicking Download Model.
3. After successful download, the Start AR button will light up.
4. You will be taken to the AR scene, where the camera will begin scanning surfaces.
5. Click the surface with your mouse or finger to display the model.
6. You can zoom in/out or rotate the model using the default gestures.
7. Click Clear All to remove all created models from the scene.

Difficulties encountered while creating the project:
0. Requires version 6000.0.43f1.
Unfortunately, this version is outdated and unsafe to use due to security issues.
https://unity.com/security/sept-2025-01
1. I have little experience with AR Foundation because I used MAXST Image Tracking. I spent time quickly researching the functionality of AR Plane Manager + AR Raycast Manager and the examples included in the project template.
2. Because I previously used TriLibCore to load models, which is a paid service, so I had to spend some time finding a plugin for loading models.
My first two searches came up: GLTFUtility (Siccity) and UnityGLTF (KhronosGroup).
Unfortunately, GLTFUtility (Siccity) didn't work for me with the models that was provided. (You can try it yourself in Test Runner -> Play Mode -> Simple AR -> GLTFUtilityTest.cs)
3. Task (2) says that when entering an AR scene, there's a "Set Model" button that's unlit until at least one plane is found. However, further on, in (3), it says that to place a model, you need to tap on the plane. I didn't understand what the "Set Model" button was supposed to do if models are placed not by pressing the button, but by tapping on the plane itself.