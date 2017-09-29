import * as electron from 'electron';
import {app, dialog, BrowserWindow} from 'electron';

import SharpAssembly from "./SharpAssembly";

var util = require('util');

var mainWindow: Electron.BrowserWindow | null = null;

app.on('window-all-closed', function() {
	if (process.platform != 'darwin') {
		app.quit();
	}
});

app.on('ready', function() {
	mainWindow = new BrowserWindow({
		width: 800,
		height: 600
	});

	mainWindow.loadURL('file://' + __dirname + '/../app/index.html');

	mainWindow.on('closed', function() {
		mainWindow = null;
	});
});


var rekoUi:SharpAssembly = new SharpAssembly("generated/assemblies/Reko.Gui.Electron.Adapter.dll");

var rootType:string = "Reko.Gui.Electron.Adapter.ElectronDecompilerDriver";

var hello = rekoUi.getFunction(rootType, "Hello");
hello();

var resolve = require('path').resolve;
var decompile = rekoUi.getFunction(rootType, "Decompile");
decompile(({
	appConfig: resolve("generated/assemblies/reko.config"),
	fileName: "E:/dec/Aberaham.exe"
}), true, function(error:any, result:any){
	console.log(error, result);
});