// -----------------Overrides-----------------

package MarchitectGameplay {
	function ShapeBase::pickup(%this,%obj,%amount) {
		//Parent::pickup(%this,%obj,%amount);
		return;
	}
	function jump(%val) {
		Parent::jump(%val);
		Marchitect.updatePlatform();
	}
	function OptionsGui::back(%this) {
		Parent::back(%this);
		MarchitectControl.push();
	}
};
activatePackage(MarchitectGameplay);

package MarchitectGeneral {
	function EditorSaveMission() {}
	
	// -----------------Replays-----------------
	
	function recordWriteHeader(%stream) {
			Parent::recordWriteHeader(%stream);
	}
	
	function PlaybackInfo::readHeader(%this) {
			Parent::readHeader(%this);
	}
};

// -----------------Controls-----------------

function MarchitectScroll(%val) {
	Marchitect.scroll(%val);
}

function KeppyMarchitect::scroll(%this, %val) {
	%val = %val / mAbs(%val);
	if(%this.heldObject)
		Marchitect.raiseObject(%val);
		
	else {
		Marchitect.updateInventory(%val);
		localClientConnection.play2D("snapSfx1");
	}
}

// -----------------Init-----------------

// tornado powered state
// draw center handles and boxes?
// trapdoor skin
// settings gui
// build/play mode
// configuration saving
// obj positions in rec
// scaling?

function KeppyMarchitect::InitObjects(%this) {
	glasses.setFadeVal(0.7);
	
	for(%i = 0; %i < MarchitectGroup.getCount(); %i++) {
		MarchitectGroup.getObject(%i).forEach("%this.MarchitectInit");
	}
	for(%i = 0; %i < MarchitectFans.getCount(); %i++) {
		DuctFan.onTrigger(MarchitectFans.getObject(%i), 0);
	}
	%this.updateInventory(1);
}

// -----------------Crosshair-----------------

ArchitectGuiContent.delete();
if(!isObject(ArchitectGuiContent)) {
	new GuiControl(ArchitectGuiContent) {
		profile = "GuiDefaultProfile";
		horizSizing = "center";
		vertSizing = "center";
		position = (PlayGui.extent.x - 100) / 2 SPC (PlayGui.extent.y - 100) / 2;
		extent = PlayGui.extent;
		minExtent = "8 8";
		visible = "1";
		helpTag = "0";
		noCursor = 1;
		new GuiBitmapCtrl(CrosshairImage) {
			profile = "GuiDefaultProfile";
			horizSizing = "center";
			vertSizing = "center";
			extent = "100 100";
			minExtent = "8 8";
			position = "0 0";
			visible = "1";
			helpTag = "0";
			bitmap = "~/data/shapes/custom/marchitect/crosshair";
			wrap = "0";
		};
	};
}

function KeppyMarchitect::respawn(%this) {
	MarchitectControl.push();
	RootGui.pushDialog(ArchitectGuiContent); 
	%this.loopObjectFinder();
}

function KeppyMarchitect::FrameAdvance(%this, %delta) {
	getMarbleCamYaw();
	getMarbleCamPitch();
	$cameraTransform = getCameraTransform();
	cameraHelper.setTransform($cameraTransform);
	$marblePos = $MP::MyMarble.getPosition();
	$marbleVel = $MP::MyMarble.getVelocity();
	
	%this.updateMounts(%delta);
	
	if(%this.heldObject)
		%this.updateObjectPosition();

	else if(%this.selectedObject)
		%this.updateIndicator();
	
	if(rotationHelper._moving)
		%this.addHelperRotation();
}

function SimObject::MarchitectInit(%this) {
	if(%this.radius $= "") {
		%this.noteRadius();
		%this.isShape = %this.dataBlock !$= "";
		%this.group = %this.getGroup();
		%this.pickupName = %this.dataBlock @ fileBase(%this.interiorFile);
	}
	%this.setTransform(Marchitect.initTransform);
	%this.setScale("1 1 1");
	if(%this.isShape) %this.setFadeVal(1);
}

// -----------------Platform-----------------

function KeppyMarchitect::lowerPlatform(%this, %time) {
	%time += 0.01;
	platform.lastHeight -= 0.05*%time;
	%this.updatePlatform = false;
	%this.lowerSchedule = %this.schedule(20, "lowerPlatform", %time);
}

function KeppyMarchitect::updatePlatform(%this) {
	%this.updatePlatform = true;
}

function KeppyMarchitect::cancelLowerPlatform(%this) {
	cancel(%this.lowerSchedule);
}

// -----------------Inventory-----------------

function SimObject::MarchitectMount(%this) {
	%this.setScale("0.2 0.2 0.2");
	if(%this.isShape) %this.setFadeVal(0.7);
}

function mLoopAround(%num, %max) {
	if(%num < 0) return %max;
	if(%num > %max) return 0;
	return %num;
}

function KeppyMarchitect::updateInventory(%this, %val) {
	%this.currentMountGroup.forEach("%this.MarchitectInit");
	
	%this.invIndex += %val;
	%this.invIndex = mLoopAround(%this.invIndex, MarchitectGroup.getCount()-1);
	
	%this.currentMountGroup = MarchitectGroup.getObject(%this.invIndex);
	%this.currentMountGroup.forEach("%this.MarchitectMount");
}

function KeppyMarchitect::updateMounts(%this, %delta) {
	// revolve objects around marble?
	
	%interpMarblePos = vectorAdd($marblePos, VectorScale($marbleVel, %delta/1000));
	%verticalSpeed = getWord($marbleVel, 2);
	%objTransform = %interpMarblePos SPC "0 0 1" SPC $cameraYaw;
	%itemGroup = %this.currentMountGroup;
	if(%itemGroup != -1) {
		for(%i = 0; %i < %itemGroup.getCount(); %i++) {
			%obj = %itemGroup.getObject(%i);
			
			%direction = -(0.5 + %i*%obj.yRadius*0.4);
			if($cameraPitch < 0)
				%direction *= -1;
			
			%newTransform = MatrixMultiply(%objTransform, "0" SPC %direction SPC "0");
			%obj.setTransform(%newTransform);	
		}
	}
	
	//if(%itemGroup != -1) {
	//	for(%i = 0; %i < %itemGroup.getCount(); %i++) {
	//		%obj = %itemGroup.getObject(%i);
	//		%transform = MatrixMultiply(
	//
	//	}
	//}
	
	if(%this.heldObject)
		%ppeTransform = %objTransform;
	else {
		%velRot = "0 0 1" SPC mAtan(getWord($marbleVel, 0), getWord($marbleVel, 1));
		%ppePosition = %interpMarblePos;
		if(%verticalSpeed < 0)
			%ppePosition = VectorAdd(%ppePosition, "0 0" SPC %verticalSpeed / -50);
			
		%ppeTransform = %ppePosition SPC %velRot;
	}
	
	hardhat.setTransform(%ppeTransform);
	glasses.setTransform(%ppeTransform);
	
	if(%this.platformEnabled) {
		%marbleHeight = getWord($marblePos, 2) - %verticalSpeed / 100;
		%heightOffset = 0.18;
		if(%this.updatePlatform && %marbleHeight > platform.lastHeight-%heightOffset)
			platform.lastHeight = %marbleHeight;
		
		%position = getWord(%interpMarblePos, 0)-0.33 SPC getWord(%interpMarblePos, 1)+0.33 SPC platform.lastHeight-%heightOffset;
		//platform.setTransform(VectorAdd(%position, getWord($marbleVel, 0) / 200 SPC getWord($marbleVel, 1) / 200 SPC 0) SPC "1 0 0 0");
		platform.setTransform(%position SPC "1 0 0 0");
	}
}

function KeppyMarchitect::stashObject(%this) {
	%obj = %this.selectedObject;
	if(!%obj) return;
	
	%this.deselectObject();
	
	%group = %obj.group;
	if(%group.getCount() == 0)
		MarchitectGroup.add(%group);
	%group.add(%obj);
	
	if(%group == %this.currentMountGroup)
		%obj.MarchitectMount();
	else
		%obj.MarchitectInit();
	
	%this.updateInventory();
	
	messageClient(localClientConnection, 'MsgItemPickup', "You stashed a" SPC %obj.pickupName @ ".");
	localClientConnection.play2D("stashSfx");
	%this.stashSchedule = %this.schedule(250, "stashObject");
}

// -----------------Select Objects-----------------

function KeppyMarchitect::loopObjectFinder(%this) {
	if(!%this.heldObject)
		%this.findObject();
	cancel(%this.finderSchedule);
	%this.finderSchedule = %this.schedule(%this.loopPeriodMS, "loopObjectFinder");
}

function SimObject::NoteRadius(%this) {
	%x = getRadius("x", %this);
	%y = getRadius("y", %this);
	%z = getRadius("z", %this);
	%this.xRadius = %x;
	%this.yRadius = %y;
	%this.zRadius = %z;
	%this.radius = %x > %y ? %x : %y;
}

function KeppyMarchitect::findObject(%this) { //credits: RandomityGuy
	%forwardDir = MatrixForward($cameraTransform);
	for(%i = 0; %i < MarchitectPlaced.getCount(); %i++) {
		%targetObject = MarchitectPlaced.getObject(%i);
		%targetVec = VectorNormalize(VectorSub(%targetObject.getWorldBoxCenter(), $cameraTransform));
		%dot = VectorDot(%targetVec, %forwardDir);
		if (%dot >= %this.angleThreshold) {
			%this.selectObject(%targetObject);
			return;
		}
	}
	%this.deselectObject();
}

function KeppyMarchitect::selectObject(%this, %obj) {
	if(%this.selectedObject == %obj)
		return;
	
	%this.deselectObject();
	%this.selectedObject = %obj;
	
	if(%obj.indicator)
		%obj.indicator.delete();
	
	// todo: change to static shape
	%indicator = new InteriorInstance() {
		scale = "0 0 0";
		interiorFile = "~/data/interiors/marchitect/shapeindicator.dif";
	};
	%obj.indicator = %indicator;
	
	%indicator.position = vectorAdd(%obj.getWorldBoxCenter(), "0 0" SPC getRadius("z", %obj));
	%indicator.moveOnPath(ScaleNode1);
	localClientConnection.play2D(selectSfx);
}

function KeppyMarchitect::updateIndicator(%this) {
	%obj = %this.selectedObject;
	%indicator = %obj.indicator;
	if(rotationHelper._moving || %this.scaleSchedule)
		%indicator.position = vectorAdd(%obj.getWorldBoxCenter(), "0 0" SPC getRadius("z", %obj));
	%dist = vectorSub($MP::MyMarble.position, %indicator.position);
	%angle = mAtan(getWord(%dist, 1), getWord(%dist, 0));
	%indicator.setTransform(%indicator.position SPC "0 0 -1" SPC %angle);
}

function KeppyMarchitect::deselectObject(%this) {
	%obj = %this.selectedObject;
	if(!%obj) return;
	
	%indicator = %obj.indicator;
	%indicator.moveOnPath("ScaleNode3");
	%indicator.schedule(ScaleNode3.timeToNext, "delete");
	%obj.indicator = "";
	%this.selectedObject = "";
	localClientConnection.play3D(deselectSfx, %obj.getWorldBoxCenter());
}

// -----------------Move Objects-----------------

function KeppyMarchitect::holdFromInventory(%this) {
	// todo: drop to ground?
	// set at marble's height
	%this.deselectObject();
	%this.lastYaw = 0;
	
	%group = %this.currentMountGroup;
	%obj = %group.getObject(0);
	
	MarchitectPlaced.add(%obj);
	if(%obj.isShape) %obj.setFadeVal(1);
	
	%position = VectorAdd($cameraTransform, MatrixMulVector($cameraTransform, "0" SPC 4 + %obj.radius SPC "0"));
	%position = setWord(%position, 2, getWord($marblePos, 2));
	%obj.initialTransform = %position;
	// todo: abstract snap turns
	// change variable names
	%turn = $cameraYaw;
	if(%this.snapToGrid)
		 %turn = mRound(%yaw / ($pi/8)) * $pi/8;
	%obj.initialRotation = "0 0 1" SPC %turn;
	%obj.initialTurn = %turn;
	%obj.lastYaw = $cameraYaw;
	%obj.lastRotation = "";
	%obj.initialCameraTransform = $cameraTransform;
	
	%this.heldObject = %obj;
	
	if(%group.getCount() == 0) {
		MarchitectEmptyGroup.add(%group);
		%this.updateInventory(1);
	}
	
	%obj.onNextFrame("setScale", "1 1 1");
}

function KeppyMarchitect::RoundAngle(%this, %angle) {
	return mRound(%angle / (%this.rotationAngle)) * %this.rotationAngle;
}

function KeppyMarchitect::updateObjectPosition(%this) {
	// todo: fix turn when enabling snap
	// grid shape?
	%obj = %this.heldObject;
	
	%change = MatrixDivide($cameraTransform, %obj.initialCameraTransform);
	%transform = MatrixMultiply(%change, %obj.initialTransform);
		
	%position = getWords(%transform, 0, 2);
	%turn = $cameraYaw - %obj.lastYaw;
	%obj.turn = %turn;

	if(%this.snapToGrid) {
		%rounded = vectorRound(vectorScale(%position, (1 / %this.gridSize)));
		%position = vectorScale(%rounded, %this.gridSize);
		%turn = mRound(%turn / (%this.rotationAngle)) * %this.rotationAngle;
		// different sound for rotation?
		if(%obj.lastPosition !$= %position)
			localClientConnection.play3D("snapSfx" @ getRandom(1, 4), %obj.getWorldBoxCenter());
		if(%obj.lastTurn !$= %turn)
			localClientConnection.play3D("raiseSfx", %obj.getWorldBoxCenter());
			
		%obj.lastPosition = %position;
		%obj.lastTurn = %turn;
	}
	%rotation = RotMultiply("0 0 1" SPC %turn, %obj.initialRotation);

	%obj.setTransform(%position SPC %rotation);

}

function KeppyMarchitect::playPickupSfx(%this, %obj) {
	%sound = %obj.pickupSound; // todo: store where?
	if(%sound $= "")
		%sound = %this.defaultPickupSound;
	localClientConnection.play3D(%sound, %obj.getWorldBoxCenter());
}

function KeppyMarchitect::holdObject(%this) {
	%obj = %this.selectedObject;
	if(!%obj) return;

	%obj.initialTransform = %obj.getTransform();
	%obj.initialRotation = %obj.getRotation();
	%obj.initialPosition = %obj.getPosition();
	%obj.initialTurn = %obj.turn;
	%obj.initialCameraTransform = $cameraTransform;
	%obj.lastYaw = $cameraYaw;
	
	%obj.distance = VectorDist(%obj.position, $cameraTransform);
	%obj.lastRotation = %obj.getRotation();
	%this.heldObject = %obj;
	%this.lastYaw = $cameraYaw;
	
	messageClient(localClientConnection, 'MsgItemPickup', "You picked up a" SPC %obj.pickupName @ "!");
	%this.playPickupSfx(%obj);
	%this.deselectObject();
}

function KeppyMarchitect::dropObject(%this) {
	%obj = %this.heldObject;
	if(!%obj) return;
	
	%this.heldObject = "";
	localClientConnection.play2D("dropSfx");
}

function KeppyMarchitect::raiseObject(%this, %val) {
	// todo: different tick when moving forward or back
	if(!%this.heldObject) return;
	
	%obj = %this.heldObject;
	%change = %this.gridSize*%val;
	
	if(%this.proximitySelect)
		%obj.offset = vectorAdd(%obj.offset, "0 0" SPC %change);
	
	else {
		%offset = MatrixMulVector(%obj.initialCameraTransform, "0" SPC %this.gridSize*%val SPC "0");
		%obj.initialTransform = VectorAdd(%offset, getWords(%obj.initialTransform, 0, 2)) SPC getWords(%obj.initialTransform, 3, 6);
	}
	
	localClientConnection.play3D(raiseSfx, %obj.getWorldBoxCenter());
}

function KeppyMarchitect::ScaleObject(%this, %val) {
	// Scale meter?
	%obj = %this.selectedObject;
	if(!%obj) {
		%this.StopScaling();
		return;
	}
	if(!%this.scaleHandle)
		Marchitect.scaleHandle = alxPlay(ShockLoopSfx);
	
	%scale = VectorAdd(%obj.scale, VectorScale("0.01 0.01 0.01", %val));
	%num = FirstWord(%scale);
	if(%num >= 0.5 && %num <= 2) {
		%obj.setScale(%scale);
		%this.scaleSchedule = %this.schedule(10, "ScaleObject", %val);
	}
	else
		%this.StopScaling();
}

function KeppyMarchitect::StopScaling(%this) {
	if(!%this.scaleSchedule) return;
	cancel(%this.scaleSchedule);
	alxStop(%this.scaleHandle);
	%this.scaleHandle = "";
	%this.scaleSchedule = "";
}


// -----------------Rotate Objects-----------------

// account for yaw
function KeppyMarchitect::addHelperRotation(%this) {
	%obj = rotationHelper.obj;
	%rotation = rotationHelper._moving ? rotationHelper.getRotation() : %this.currentRotation;
	%rotation = RotMultiply(%rotation, RotInverse(%this.currentTurn));
	%rotation = RotMultiply(%rotation, %obj.lastRotation);
	
	%obj.setTransform(%obj.initialPosition SPC %rotation);
}

function KeppyMarchitect::stopRotating(%this, %obj) {
	//if(!rotationHelper._moving) return;
	
	cancel(%this.moveSchedule);
	rotationHelper.cancelMoving();
	%this.addHelperRotation();
}

function KeppyMarchitect::rotateHeldObject(%this, %axis) {
	// todo: account for obj center
	%obj = %this.selectedObject;
	if(!%obj) return;
	rotationHelper.obj = %obj;
	if(rotationHelper._moving)
		%this.stopRotating();
	
	%obj.lastRotation = %obj.getRotation();
	%obj.initialPosition = %obj.getPosition();
	
	%direction = %this.shiftVal;
	%rotation = setWord("0 0 0", %axis, %direction) SPC %this.rotationAngle;
	
	// todo: snap angle
	%yaw = $cameraYaw;
	if(%this.snapToGrid)
		%yaw = mRound(%yaw / ($pi/8)) * $pi/8;
	
	%turn = "0 0 1" SPC %yaw;

	%rotation = RotMultiply(%turn, %rotation);
	%this.currentRotation = %rotation;
	%this.currentTurn = %turn;
	RotationNode1.setTransform("0 0 0" SPC %turn);
	RotationNode2.setTransform("0 0 0" SPC %rotation);
	
	
	
	rotationHelper.moveOnPath(RotationNode1);
	%this.moveSchedule = %this.schedule(RotationNode1.timeToNext, "stopRotating");
	
	localClientConnection.play2D("rotateSfx");
}





// -----------------End-----------------

function KeppyMarchitect::leave(%this) {
	deactivatePackage(MarchitectGameplay);
	RootGui.popDialog(ArchitectGuiContent);
	hardhatData.delete();
	glassesData.delete();
}


MarchitectControl.bindCmd(keyboard, "lshift", "Marchitect.shiftVal = -1;", "Marchitect.shiftVal = 1;");
MarchitectControl.bindCmd(keyboard, "lcontrol", "Marchitect.lowerPlatform(1);", "Marchitect.cancelLowerPlatform();");
MarchitectControl.bindCmd(keyboard, "up", "Marchitect.rotateHeldObject(0);", "");
MarchitectControl.bindCmd(keyboard, "down", "Marchitect.rotateHeldObject(1);", "");
MarchitectControl.bindCmd(keyboard, "right", "Marchitect.rotateHeldObject(2);", "");
MarchitectControl.bindCmd(keyboard, "e", "Marchitect.stashObject();", "cancel(Marchitect.stashSchedule);");
MarchitectControl.bindCmd(keyboard, "g", "Marchitect.snapToGrid = !Marchitect.snapToGrid;", "");
MarchitectControl.bindCmd(keyboard, "1", "Marchitect.ScaleObject(1);", "Marchitect.StopScaling();");
MarchitectControl.bindCmd(keyboard, "2", "Marchitect.ScaleObject(-1);", "Marchitect.StopScaling();");
MarchitectControl.bindCmd(keyboard, "y", "Marchitect.platformEnabled = !Marchitect.platformEnabled; Platform.lastHeight = -100;", "");
MarchitectControl.bindCmd(keyboard, "left", "", "");
MarchitectControl.bindCmd(mouse, "button0", "Marchitect.holdObject();", "Marchitect.dropObject();");
MarchitectControl.bindCmd(mouse, "button1", "Marchitect.holdFromInventory();", "");
MarchitectControl.bind(mouse, "zaxis", "MarchitectScroll");

MarchitectControl.bindCmd(keyboard, "j", "exec (\"platinum/data/missions/custom/marchitectScript.mis\");", "");