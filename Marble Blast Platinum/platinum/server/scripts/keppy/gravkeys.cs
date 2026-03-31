function clientCbOnRespawn() {
	activatePackage(gravkeys);
	gravkeyControl.push();
}

function clientCbOnFrameAdvance() {
	if(!$Game::Menu) {
		updateGravityDirection();
	}
}

// change to use only 2 path nodes
package gravkeys {
	
	function incYaw(%val) {
		missionState.yaw += $pi*%val/1024;
	}

	function updateGravityDirection() {
		%rot1 = RotMultiply(gravityHelper1.getRotation(), gravityHelper3.getRotation());
		%rot2 = RotMultiply(gravityHelper2.getRotation(), gravityHelper4.getRotation());
		%rot3 = RotMultiply(%rot1, %rot2);
		
		%rot4 = RotMultiply("0 0 1" SPC missionState.yaw, %rot3);
		%skyrot = RotMultiply(%rot4, RotInverse("0 0 1" SPC missionState.yaw));
		skyball.setTransform("0 0 0" SPC %skyrot);
		%rot5 = RotMultiply(%rot4, "1 0 0" SPC $pi); 
		%rot6 = vectorOrthoBasis(%rot5);
		
		localClientConnection.setGravityDir(%rot6, true, %rot5);
	}
	
	function holdDirection(%id) {
		%FirstNode = "FirstNode" @ %id;
		%ThirdNode = "ThirdNode" @ %id;
		%gravityHelper = "gravityHelper" @ %id;
		
		%FirstNode.setTransform(%gravityHelper.getTransform());
		
		%distance = firstWord(%ThirdNode.position) - firstWord(%FirstNode.position);
		%FirstNode.timeToNext = 300 * %distance;
		
		%gravityHelper.cancelMoving();
		%FirstNode.nextNode = %ThirdNode;
		%gravityHelper.moveOnPath(%FirstNode);
	}
	
	function releaseDirection(%id) {
		%FirstNode = "FirstNode" @ %id;
		%SecondNode = "SecondNode" @ %id;
		%gravityHelper = "gravityHelper" @ %id;
		
		%SecondNode.setTransform(%gravityHelper.getTransform());
		%FirstNode.setTransform("0 0 0 1 0 0 0");
		
		%distance = firstWord(%SecondNode.position);
		%SecondNode.timeToNext = 300 * %distance;
		
		%gravityHelper.cancelMoving();
		%FirstNode.nextNode = "";
		%gravityHelper.moveOnPath(%SecondNode);
	}
	
	function createDirectionObjects() {
		for(%i = 1; %i <= 4; %i++) {
			new TSStatic("gravityHelper" @ %i) { 
				scale = "0 0 0"; 
				shapeName = "~/data/shapes/items/antigravity.dts"; 
			};
			new StaticShape("FirstNode" @ %i) {
				dataBlock = "PathNode";
					nextNode = "ThirdNode" @ %i;
					Smooth = "1";
					SmoothEnd = "1";
					SmoothStart = "1";
					useScale = "0";
			};
			new StaticShape("SecondNode" @ %i) {
				dataBlock = "PathNode";
					nextNode = "FirstNode" @ %i;
					Smooth = "1";
					SmoothEnd = "1";
					SmoothStart = "1";
					useScale = "0";
					
			};
			new StaticShape("ThirdNode" @ %i) {
				position = "1 0 0";
				rotation = getField(missionList.directions, %i);
				orotation = getField(missionList.directions, %i);
				dataBlock = "PathNode";
					Smooth = "1";
					SmoothEnd = "1";
					SmoothStart = "1";
					useScale = "0";
			};
		}
	}
	
	function GameConnection::onClientLeaveGame(%this) {
		Parent::onClientLeaveGame(%this);
		gravkeyControl.pop();
		deactivatePackage(gravkeys);
	}
};

activatePackage(gravkeys);