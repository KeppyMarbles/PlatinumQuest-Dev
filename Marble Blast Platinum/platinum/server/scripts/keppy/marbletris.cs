datablock AudioProfile(swapSfx : ReadyVoiceSfx)    { filename = "~/data/sound/ap_mbg/ButtonPress.wav"; };
datablock AudioProfile(tickSfx2 : ReadyVoiceSfx)   { filename = "~/data/sound/custom/keppyTick2.wav"; };
datablock AudioProfile(snapSfx1 : ReadyVoiceSfx)   { filename = "~/data/sound/custom/keppySnap1.wav"; };
datablock AudioProfile(snapSfx2 : ReadyVoiceSfx)   { filename = "~/data/sound/custom/keppySnap2.wav"; };
datablock AudioProfile(snapSfx3 : ReadyVoiceSfx)   { filename = "~/data/sound/custom/keppySnap3.wav"; };
datablock AudioProfile(snapSfx4 : ReadyVoiceSfx)   { filename = "~/data/sound/custom/keppySnap4.wav"; };
datablock AudioProfile(score1Sfx : ReadyVoiceSfx)  { filename = "~/data/sound/custom/gemScore1.wav"; };
datablock AudioProfile(score1Sfx : ReadyVoiceSfx)  { filename = "~/data/sound/custom/gemScore1.wav"; };
datablock AudioProfile(score2Sfx : ReadyVoiceSfx)  { filename = "~/data/sound/custom/gemScore2.wav"; };
datablock AudioProfile(score3Sfx : ReadyVoiceSfx)  { filename = "~/data/sound/custom/gemScore3.wav"; };
datablock AudioProfile(score4Sfx : ReadyVoiceSfx)  { filename = "~/data/sound/custom/gemScore4.wav"; };
datablock AudioProfile(score5Sfx : ReadyVoiceSfx)  { filename = "~/data/sound/custom/gemScore5.wav"; };
datablock AudioProfile(score6Sfx : ReadyVoiceSfx)  { filename = "~/data/sound/custom/gemScore6.wav"; };
datablock AudioProfile(score7Sfx : ReadyVoiceSfx)  { filename = "~/data/sound/custom/gemScore7.wav"; };
datablock AudioProfile(score8Sfx : ReadyVoiceSfx)  { filename = "~/data/sound/custom/gemScore8.wav"; };
datablock AudioProfile(score9Sfx : ReadyVoiceSfx)  { filename = "~/data/sound/custom/gemScore9.wav"; };
datablock AudioProfile(score10Sfx : ReadyVoiceSfx) { filename = "~/data/sound/custom/gemScore10.wav"; };

datablock StaticShapeData(MarbletrisBlock) { shapeFile = "~/data/shapes/custom/marbletris/marbletrisBlock3.dts"; };

datablock ItemData(GemItemPinkTetris : GemItemPink)   { huntExtraValue = 7; };
datablock ItemData(GemItemBlackTetris : GemItemBlack) { huntExtraValue = 8; };

datablock ParticleData(GemParticleGreenTetris : GemParticleGreen)              { sizes[0] = 5; };
datablock ParticleEmitterData(GemEmitterGreenTetris : GemEmitterGreen)         { particles = "GemParticleGreenTetris"; };
datablock ParticleData(GemParticleRedTetris : GemParticleRed)                  { sizes[0] = 5; };
datablock ParticleEmitterData(GemEmitterRedTetris : GemEmitterRed)             { particles = "GemParticleRedTetris"; };
datablock ParticleData(GemParticleBlueTetris : GemParticleBlue)                { sizes[0] = 5; };
datablock ParticleEmitterData(GemEmitterBlueTetris : GemEmitterBlue)           { particles = "GemParticleBlueTetris"; };
datablock ParticleData(GemParticleBlackTetris : GemParticleBlack)              { sizes[0] = 5; };
datablock ParticleEmitterData(GemEmitterBlackTetris : GemEmitterBlack)         { particles = "GemParticleBlackTetris"; };
datablock ParticleData(GemParticlePlatinumTetris : GemParticlePlatinum)        { sizes[0] = 5; };
datablock ParticleEmitterData(GemEmitterPlatinumTetris : GemEmitterPlatinum)   { particles = "GemParticlePlatinumTetris"; };
datablock ParticleData(GemParticleYellowTetris : GemParticleYellow)            { sizes[0] = 5; };
datablock ParticleEmitterData(GemEmitterYellowTetris : GemEmitterYellow)       { particles = "GemParticleYellowTetris"; };
datablock ParticleData(GemParticlePurpleTetris : GemParticlePurple)            { sizes[0] = 5; };
datablock ParticleEmitterData(GemEmitterPurpleTetris : GemEmitterPurple)       { particles = "GemParticlePurpleTetris"; };
datablock ParticleData(GemParticleOrangeTetris : GemParticleOrange)            { sizes[0] = 5; };
datablock ParticleEmitterData(GemEmitterOrangeTetris : GemEmitterOrange)       { particles = "GemParticleOrangeTetris"; };
datablock ParticleData(GemParticlePinkTetris : GemParticlePink)                { sizes[0] = 5; };
datablock ParticleEmitterData(GemEmitterPinkTetris : GemEmitterPink)           { particles = "GemParticlePinkTetris"; };
datablock ParticleData(GemParticleTurquoiseTetris : GemParticleTurquoise)      { sizes[0] = 5; };
datablock ParticleEmitterData(GemEmitterTurquoiseTetris : GemEmitterTurquoise) { particles = "GemParticleTurquoiseTetris"; };

datablock ParticleData(TetrisBounceParticle : BounceParticle) { 
	sizes[0] = 0.75;
	sizes[1] = 0.75;
	sizes[2] = 0.75;
};
datablock ParticleEmitterData(TetrisBounceEmitter : MarbleBounceEmitter) { particles = "TetrisBounceParticle"; };

//todo: allow spam
//TODO the marble moves now in freecam, we'll need to freeze it or get the new camera transform
//TODO change on finishing
//TODO we may need to (smoothly) change user FOV
function KeppyMarbletris::swap(%this) {
	debug("Switching mode");
	if(isEventPending(%this.cameraSch))
		return;
	%this.tetrisMode = !%this.tetrisMode;
	if(%this.tetrisMode) {
		debug("Switching to tetris mode");
		TetrisControl.push();
		marbletrisCameraPath1.setTransform(getCameraTransform());
		%this.lastMarblePos = $MP::MyMarble.getPosition();
		//LocalClientConnection.freezeMarble(true);
		LocalClientConnection.setToggleCamera(true);
		LocalClientConnection.camera.moveOnPath(marbletrisCameraPath1);
		%this.cameraSch = LocalClientConnection.camera.schedule(marbletrisCameraPath1.timeToNext, "moveOnPath", "sillyPath");
		//localClientConnection.camera.schedule(marbletrisCameraPath1.timeToNext, "cancelMoving");
		%this.lowerCTLoop();
		%this.dropTetrimino.hide(false);
	}
	else {
		debug("Switching to marble mode");
		cancel(%this.lowerCTSchedule);
		TetrisControl.pop();
		//marbletrisCameraPath1.setTransform(getCameraTransform());
		marbletrisCameraPath1.nextNode = "marbletrisCameraPath1";
		marbletrisCameraPath2.nextNode = "marbletrisCameraPath1";
		%cameraOffset = VectorSub($MP::MyMarble.getPosition(), %this.lastMarblePos);
		marbletrisCameraPath1.setPosition(VectorAdd(marbletrisCameraPath1.position, %cameraOffset));
		
		LocalClientConnection.camera.moveOnPath(marbletrisCameraPath2);
		LocalClientConnection.schedule(marbletrisCameraPath2.timeToNext, "setToggleCamera", false); // TODO cancel this
		%this.cameraSch = LocalClientConnection.camera.schedule(marbletrisCameraPath1.timeToNext, "cancelMoving");
		//LocalClientConnection.schedule(marbletrisCameraPath1.timeToNext, "freezeMarble", false);
		%this.dropTetrimino.hide(true);
	}
}

function SimObject::setPosition(%this, %pos) {
	%this.setTransform(%pos SPC %this.getRotation());
}

function SimObject::setRotation(%this, %rot) {
	%this.setTransform(%this.getPosition() SPC %rot);
}

function KeppyMarbletris::playSnapSfx(%this) {
	%sfx = "snapSfx" @ getRandom(1, 4);
	debug("Playing sfx" SPC %sfx);
	alxPlay(%sfx);
}

function KeppyMarbletris::playDropSfx(%this) {
	%sfx = "bounce" @ getRandom(1, 4) @ "Sfx";
	debug("Playing sfx" SPC %sfx);
	alxPlay(%sfx);
}

// -----------------Controls-----------------

function debug(%text) {
	//if($DEBUG && $lastDebug !$= %text)
	//  echo(%text);
	//$lastDebug = %text;
	if($DEBUG) 
		echo("DEBUG:" SPC %text);
}

function KeppyMarbletris::holdLower(%this) {
	debug("Holding lower CT");
	%this.lowerActiveMino();
	%this.currentTetrimino.lowerSchedule = %this.schedule(50, "holdLower");
	%this.playSnapSfx();
}

function MarbletrisScroll(%val) {
	Marbletris.currentTetrimino.rotate(%val / mAbs(%val));
}

function MarbletrisX(%val) {
	Marbletris.mouseShiftCT(%val);
}

//function holdshiftCT1(%val) {
//  shiftCT(%val);
//  CT.moveSchedule1 = schedule(300, 0, "holdshiftCT2", %val);
//}
//
//function holdshiftCT2(%val) {
//  shiftCT(%val);
//  CT.moveSchedule2 = schedule(100, 0, "holdshiftCT2", %val);
//}
//
//function releaseshiftCT(%val) {
//  cancel(CT.moveSchedule1);
//  cancel(CT.moveSchedule2);
//}

// -----------------Start-----------------

function KeppyMarbletris::onMissionReset(%this) {
	debug("Resetting script");

	$Game::GemCount = 999; // todo, use actual gem count, but only finish if board is done

	TetrisControl.pop();
	MarbletrisControl.push();

	MarbletrisObjects.clear();
	MarbletrisGems.clear();
	MarbletrisTetriminos.clear();

	%this.delete();

	%this = new ScriptObject(Marbletris) {
		class = "KeppyMarbletris";
		//fallDelay = 500;
		bagPos = 8;
		level = 1;
		gemLevelRequirement = 10;
	};

	%this.currentTetrimino = new ScriptObject() { 
		superClass = "Tetrimino";
		class = "CurrentTetrimino";
		//game = %this;
	};
	%this.dropTetrimino = new ScriptObject() { 
		superClass = "Tetrimino";
		class = "DropTetrimino";
		//game = %this;
	};
	%this.holdTetrimino = new ScriptObject() { 
		superClass = "Tetrimino";
		class = "HoldTetrimino";
		//game = %this;
	};
	%this.nextTetrimino = new ScriptObject() { 
		superClass = "Tetrimino"; 
		class = "NextTetrimino";
		//%game = %this;
	};

	MissionGroup.add(%this);
	MarbletrisTetriminos.add(%this.currentTetrimino);
	MarbletrisTetriminos.add(%this.dropTetrimino);
	MarbletrisTetriminos.add(%this.holdTetrimino);
	MarbletrisTetriminos.add(%this.nextTetrimino);

	%this.nextTetrimino();
}

function KeppyMarbletris::createData(%this) {
	debug("Creating tetrimino data");
	for(%i = 1; %i <= 7; %i++) {
		%data = new ScriptObject();
		for(%r = 0; %r < 4; %r++) {
			for(%textRow = 0; %textRow < 5; %textRow++) {
				%line = getField(MissionList.mino[%i, %r], %textRow);
				for(%textCol = 0; %textCol < 5; %textCol++) {
					%x = %textCol; 
					%y = 4 - %textRow;
					%data.matrix[%r, %x, %y] = getWord(%line, %textCol);
				}
			}
		}
		MissionGroup.add(%data);
		MissionList.tetriminoData[%i] = %data;
	}
}

function KeppyMarbletris::generateSequence(%this) {
	debug("Generating new sequence");
	for(%i = 1; %i <= 7; %i++) {
		%seq[%i] = %i;
	}
	for(%i = 7; %i > 0; %i--) {
		%j = getRandom(1, %i);
		%temp = %seq[%i];
		%seq[%i] = %seq[%j];
		%seq[%j] = %temp;
	}
	for(%i = 1; %i <= 7; %i++) {
		%seq = %seq SPC %this.next[%i];
		%this.next[%i] = %seq[%i];
	}
}

function Tetrimino::clear(%this) {
  //for(%b = %this.start; %b; %b = %b.next)
  //  %b.onNextFrame("delete");
	for(%x = 0; %x < 5; %x++) {
		for(%y = 0; %y < 5; %y++) {
			%b = %this.matrix[%x, %y];
			if(%b) {
				%b.delete();
				%this.matrix[%x, %y] = 0;
			}
		}
	}
	//%this.id = "";
	%this.data = "";
	debug("Cleared mino" SPC %this);
	//%this.row = "";
	//%this.col = "";
}

function KeppyMarbletris::spawnMino(%this, %id) {
    %this.currentTetrimino.create(%id);
    %this.dropTetrimino.create(%id);
    %this.updateDropPosition();
}

function Tetrimino::createStatic(%this, %id, %row, %col) {
	debug("Creating static tetrimino for" SPC %this.getName());
	%this.clear();
	%this.createBlocks(%id);
	%this.row = %row;
	%this.col = %col;
	%this.updateTransform();
}

function CurrentTetrimino::create(%this, %id) {
	debug("Creating current tetrimino");
	cancel(%this.lockSch);
	
	%this.createBlocks(%id);
	%this.placed = false;
	
	%this.row = 20;
	%this.col = 5;
	%this.rotIndex = 0;
	%this.held = false;
	%this.updateTransform();
}

function DropTetrimino::create(%this, %id) {
	%this.clear();
	%this.createBlocks(%id);
	for(%b = %this.start; %b; %b = %b.next)
		%b.setFadeVal(0.5);
}

function Tetrimino::createBlocks(%this, %id) {
	debug("Creating blocks for" SPC %this SPC "with id" SPC %id);

	%this.id = %id;
	%this.data = MissionList.tetriminoData[%id];
	
	for(%x = 0; %x < 5; %x++) {
		for(%y = 0; %y < 5; %y++) {
			//if(%b = %this.matrix[%x, %y])
			//  %b.delete();

			%this.matrix[%x, %y] = 0;
			
			if(%this.data.matrix[0, %x, %y]) {
				%block = new StaticShape() {
					position = "0 0 -100";
					dataBlock = MarbletrisBlock;
					x = %x;
					y = %y;
				};
				if(%prevBlock)
					%prevBlock.next = %block;
				else
					%this.start = %block;
				
				MarbletrisObjects.add(%block);
				%this.matrix[%x, %y] = %block;
				
				%block.setSkinName("mbg" @ %this.id);
				
				%prevBlock = %block;
				
				debug("Created block" SPC %block);
			}
		}
	}
}

// TODO show multi next?
function KeppyMarbletris::nextTetrimino(%this) {
	debug("----- Creating next tetrimino -----");
	if(%this.bagPos < 7)
		%this.bagPos++;
	else {
		%this.bagPos = 1;
		%this.generateSequence();
	}
	debug("Updating bagPos to" SPC %this.bagPos);
	%this.spawnMino(%this.next[%this.bagPos]);
	%this.nextTetrimino.createStatic(%this.next[%this.bagPos+1], 15, 11);
}

function KeppyMarbletris::lowerCTLoop(%this) {
	%this.currentTetrimino.lower();
	cancel(%this.lowerCTSchedule);
	//%this.lowerCTSchedule = %this.schedule(%this.fallDelay, "lowerCTLoop");
	if(%this.level > 10)
		%delay = 50;
	else
		%delay = getWord(MissionList.fallDelays, %this.level-1);
	%this.lowerCTSchedule = %this.schedule(%delay, "lowerCTLoop");
}


// -----------------Move Tetrominos-----------------

function KeppyMarbletris::mouseShiftCT(%this, %val) {
	%lastDirection = %this.mouseDirection;
	if(%val > 0)
		%this.mouseDirection = 1;
	else
		%this.mouseDirection = -1;
	if(%lastDirection != %this.mouseDirection)
		%this.shiftVal = 0;
	
	%this.shiftVal += %val;
	// Todo: sensitivity?
	if(%this.shiftVal < -50)
		%this.shiftCT(-1);
	else if (%this.shiftVal > 50)
		%this.shiftCT(1);

	if(%this.shiftVal == 0) {
		cancel(%this.resetShiftValSch);
		%this.resetShiftValSch = %this.schedule(500, "resetShiftVal");
	}
}

function KeppyMarbletris::resetShiftVal(%this) {
	%this.shiftVal = 0;
	debug("Reset shift val");
}

function KeppyMarbletris::shiftCT(%this, %val) {
	debug("Shifting CT with val" SPC %val);
	%this.shiftVal = 0;
	if(%this.currentTetrimino.shift(%val)) {
		%this.playSnapSfx();
		%this.dropTetrimino.update();
	}
}

function CurrentTetrimino::shift(%this, %val) {
	for(%b = %this.start; %b; %b = %b.next) {
		if(!isObject(%b)) {
			debug("CT block doesn't exist?");
			return false;
		}
		%nextX = %b.xPos + %val;
		if(%nextX < 0 || %nextX > 9 || Marbletris.board[%nextX, %b.yPos]) {
			debug(%b SPC "collided with something");
			return false;
		}
	}
	
	cancel(%this.lockSch);
	%this.col += %val;
	%this.updateTransform();
	return true;
}

function Tetrimino::updateTransform(%this) {
	for(%b = %this.start; %b; %b = %b.next) {
		//%rot = isObject(%this) ? %this.rot : %b.getRotation();
		%b.xPos = %b.x + %this.col;
		%b.yPos = %b.y + %this.row;
		%transform = %b.xPos*2 SPC %b.yPos*2 SPC "0 0 0 1" SPC %this.rotIndex*$pi_2;
		%b.setTransform(%transform);
	}
}

function KeppyMarbletris::lowerActiveMino(%this) {
	//debug("Lowering CT");
	//if(%this.currentTetrimino.placed) {
	//	debug("Can't lower placed CT");
	//	return;
	//}
	
	for(%b = %this.currentTetrimino.start; %b; %b = %b.next) {
		if(!isObject(%b)) {
			debug("CT block doesn't exist?");
			return;
		}
		if(%this.board[%b.xPos, %b.yPos-1] || %b.yPos-1 < 0) {
			debug("Scheduling CT placement");
			%this.currentTetrimino.lockSch = %this.schedule(500, "placeActiveMino"); //TODO
			return;
		}
	}

	for(%b = %this.currentTetrimino.start; %b; %b = %b.next) {
		//debug("Creating trail particle for" SPC %this);
		spawnEmitter(200, MarbleTrailEmitter, %b.position, false);
	}
	
	%this.currentTetrimino.row--;
	%this.currentTetrimino.updateTransform();
	
	//if(%user)
	//	%this.currentTetrimino.dropScore++;
}

// -----------------Place Tetrominos-----------------

function KeppyMarbletris::updateDropPosition(%this) {
	%this.dropTetrimino.col = %this.currentTetrimino.col;
	%this.dropTetrimino.rotIndex = %this.currentTetrimino.rotIndex;

	%minDist = 25;
	for(%b = %this.currentTetrimino.start; %b; %b = %b.next) {
		for(%i = %b.yPos; %i >= -1; %i--) {
			if(%this.board[%b.xPos, %i] || %i == -1) {
				%dist = %b.yPos-%i-1;
				if(%dist < %minDist)
					%minDist = %dist;
			}
		}
	}
	
	%this.dropTetrimino.row = %this.currentTetrimino.row - %minDist;
	%this.dropTetrimino.updateTransform();
}

function Tetrimino::getHorizontalExtents(%this) {
    %maxX = 0; %minX = 99;
    for(%b = %this.start; %b; %b = %b.next) {
        if(%b.xPos > %maxX) %maxX = %b.xPos;
        if(%b.xPos < %minX) %minX = %b.xPos;
    }
    return %minX SPC %maxX;
}

function KeppyMarbletris::dropActiveMino(%this) {
    debug("Dropping active tetrimino");

    // Get the piece-specific bounds (Logic remains in the piece)
    %bounds = %this.currentTetrimino.getHorizontalExtents();
    %minX = getWord(%bounds, 0);
    %maxX = getWord(%bounds, 1);

    // Orchestrate effects using BOTH pieces
    for(%x = %minX; %x <= %maxX; %x++) {
        for(%y = %this.dropTetrimino.row; %y < %this.currentTetrimino.row; %y++) {
            spawnEmitter(200, MarbleTrailEmitter, %x*2 SPC %y*2 SPC 0, false);
        }
    }

    // Update scoring and position
    %this.currentTetrimino.dropScore += (%this.currentTetrimino.row - %this.dropTetrimino.row) * 2;
    %this.currentTetrimino.row = %dt.row;
    %this.currentTetrimino.updateTransform();

	for(%b = %this.currentTetrimino.start; %b; %b = %b.next) {
		if(%this.board[%b.xPos, %b.yPos-1] || %b.yPos == 0) {
			debug("Creating bounce particle for" SPC %b);
			spawnEmitter(200, TetrisBounceEmitter, vectorAdd(%b.position, -1 SPC -1 SPC 1), false);
		}
	}

    %this.placeActiveMino();
}

function KeppyMarbletris::placeActiveMino(%this) {
	debug("Placing CT");
	if(%this.currentTetrimino.placed) {
		debug("CT already placed");
		return;
	}
	cancel(%this.currentTetrimino.lockSch);
	cancel(%this.currentTetrimino.lowerSchedule);
	
	%this.currentTetrimino.placed = true;

	for(%b = %this.currentTetrimino.start; %b; %b = %b.next)
		%this.board[%b.xPos, %b.yPos] = %b;

	for(%y = 0; %y < 25; %y++) {
		%clear = true;
		for(%x = 0; %x < 10; %x++) {
			// fix?
			%clear = %this.board[%x, %y];
			if(!%clear) {
				debug("No clear on row" SPC %y @ "; no block at" SPC %x SPC %y);
				break;
			}
		}
		if(%clear) {
			debug("Clear on row" SPC %y);
			%clears = %clears SPC %y;
		}
	}

	if(%clears $= "") {
		if(%this.currentTetrimino.row >= 20) {
			
			cancel(%this.lowerCTSchedule);
			if(%this.getBoardValue() == 0)
				endGameSetup();
		}
		else
			%this.nextTetrimino();
		%this.streak = 0;
		%this.msg = "";
	}
	else
		%this.clearRows(ltrim(%clears));
	
	%this.playDropSfx();

	//%this.maxHeight = max(%this.maxHeight, %this.getBoardMaxHeight());
}

// -----------------Hold Tetrominos-----------------

function KeppyMarbletris::holdActiveMino(%this) {
    %ct = %this.currentTetrimino;
    %ht = %this.holdTetrimino;

    if (%this.currentTetrimino.held || %this.currentTetrimino.id == %this.holdTetrimino.id) 
		return;

    %oldID = %this.currentTetrimino.id;
    %this.currentTetrimino.clear();

    if (%this.holdTetrimino.id)
        %this.spawnMino(%this.holdTetrimino.id);
    else
        %this.nextTetrimino(); 

    %this.currentTetrimino.held = true;
    %ht.createStatic(%oldID, 15, -6);
    LocalClientConnection.play2D("swapSfx");
}
// -----------------Rotate Tetrominos-----------------

function Tetrimino::setRotation(%this, %rot) {
	%b = %this.start;
	for(%x = 0; %x < 5; %x++) {
		for(%y = 0; %y < 5; %y++) {
			if(%this.data.matrix[%rot, %x, %y]) {
				%this.matrix[%x, %y] = %b;
				%b.x = %x;
				%b.y = %y;
				%b = %b.next;
				if(!%b) {
					%this.rotIndex = %rot;
					%this.updateTransform();
					return;
				}
			}
			else {
				%this.matrix[%x, %y] = 0;
			}
		}
	}
}

function Tetrimino::checkCollision(%this, %rotIndex, %colOffset, %rowOffset) {
	for(%x = 0; %x < 5; %x++) {
		for(%y = 0; %y < 5; %y++) {
			if(%this.data.matrix[%rotIndex, %x, %y]) {
				%checkX = %x + %this.col + %colOffset;
				%checkY = %y + %this.row + %rowOffset;
				
				// Check walls, floor, and existing blocks
				if(%checkX < 0 || %checkX > 9 || %checkY < 0 || Marbletris.board[%checkX, %checkY]) {
					return true; // Collision found!
				}
			}
		}
	}
	return false; 
}

function CurrentTetrimino::rotate(%this, %val) {
	debug("Rotating" SPC %this SPC "with val" SPC %val);
	%nextRotIndex = (((%this.rotIndex + %val) % 4) + 4) % 4;
	debug("From" SPC %this.rotIndex SPC "to" SPC %nextRotIndex SPC "on id" SPC %this.id);
	
	switch(%this.id) {
		case 4: // O piece
			%kickData = "0 0";
		case 1: // I piece
			%kickData = MissionList.SRSKick_I[%this.rotIndex, %nextRotIndex];
		default:
			%kickData = MissionList.SRSKick_Normal[%this.rotIndex, %nextRotIndex];
	}

	%testCount = (%kickData) / 2;
	for(%i = 0; (%test = getField(%kickData, %i)) !$= ""; %i++) {
		%testX = firstWord(%test);
		%testY = restWords(%test);
		if(!%this.checkCollision(%nextRotIndex, %testX, %testY)) {
			// Found a valid position!
			%kicked = true;
			%kickX = %testX;
			%kickY = %testY;
			break; 
		}
	}

	if(!%kicked) {
		debug("Rotation failed - no valid SRS kicks available");
		return;
	}
	debug("Kick X:" SPC %kickX SPC "Kick Y:" SPC %kickZ);

	%this.col += %kickX;
	%this.row += %kickY;
	
	cancel(%this.lockSch);
	
	%this.setRotation(%nextRotIndex);
	Marbletris.dropTetrimino.setRotation(%nextRotIndex);
	Marbletris.updateDropPosition();
	
	alxPlay(tickSfx2);
}

function KeppyMarbletris::dropBoard(%this, %rows) {
	debug("Dropping board with rows" SPC %rows);
	%this.playDropSfx();
	for(%i = 0; %i < getWordCount(%rows); %i++) {
		%row = getWord(%rows, %i)-%i;
		for(%y = %row; %y < 20; %y++) {
			for(%x = 0; %x < 10; %x++) {
				%this.board[%x, %y] = %this.board[%x, %y+1];
			}
		}
	}
	%this.updateBoard();
}

function KeppyMarbletris::blastBlocks(%this) { //TODO base off of blast meter
	debug("Blasting blocks");
	%marblePos = localClientConnection.player.position;
	%x = mRound(getWord(%marblePos, 0) / 2 - 0.5);
	%y = mRound(getWord(%marblePos, 1) / 2 - 0.5);
	for(%i = -1; %i <= 1; %i++) {
		for(%j = -1; %j <= 1; %j++) {
			%block = %this.board[%x+%i, %y+%j];
			if(%block)
				%this.destroyBlock(%block);
			%this.board[%x+%i, %y+%j] = "";
				//%block.getDataBlock().destroy(%block);
		}
	}
}

function KeppyMarbletris::destroyBlock(%this, %block) {
	ServerPlay3D("bounce" @ getRandom(1, 4) @ "Sfx", %block.position);
	spawnEmitter(25, LandMineSparkEmitter, %block.position, false);
	%block.delete();
}

//function MarbletrisBlock::destroy(%this, %block) {
//  ServerPlay3D("bounce" @ getRandom(1, 4) @ "Sfx", %block.position);
//  spawnEmitter(25, LandMineSparkEmitter, %block.position, false);
//  %block.delete();
//}

// make it based on marble pos
function KeppyMarbletris::destroyBoard(%this) {
	// get max y
	debug("Destroying board");
	%pos = $MP::MyMarble.getPosition();
	for(%x = 0; %x < 10; %x++) {
		for(%y = 0; %y < 25; %y++) {
			%b = %this.board[%x, %y];
			if(%b)
				%this.schedule(2000-vectorDist(%b.position, %pos)*100, "destroyBlock", %b);
				//%b.schedule(2000-vectorDist(%b.position, %pos)*100, "destroy");


			%this.board[%x, %y] = "";
		}
	}
	if(!isEventPending(%this.lowerCTSchedule))
		%this.lowerCTLoop();
	
	%this.b2b = false;
	%this.streak = 0;
	%this.updateDropPosition();
}

function KeppyMarbletris::isCleared(%this) {
	for(%x = 0; %x < 10; %x++) {
		for(%y = 0; %y < 25; %y++) {
			if(isObject(%this.board[%x, %y])) {
				return false;
			}
		}
	}
	return true;
}

// todo: set transform?
function KeppyMarbletris::updateBoard(%this) {
	for(%x = 0; %x < 10; %x++) {
		for(%y = 0; %y < 25; %y++) {
			%b = %this.board[%x, %y];
			if(%b)
				%b.setPosition(%x*2 SPC %y*2 SPC 0);
		}
	}
}

function KeppyMarbletris::messageAdd(%this, %str) {
	if(%this.msg $= "")
		%this.msg = %str;
	else
		%this.msg = %this.msg SPC "+" SPC %str;
}

function KeppyMarbletris::clearRows(%this, %rows) {
	debug("Clearing rows" SPC %rows);
	%rowCount = getWordCount(%rows);
	for(%i = 0; %i < %rowCount; %i++) {
		%row = getWord(%rows, %i);
		for(%x = 0; %x < 10; %x++) {
			%this.board[%x, %row].delete();
			// use block pos?
			spawnEmitter(200, LandMineSparkEmitter, %x*2 SPC %row*2 SPC 0, false);
			//%this.board[%x, %row] = "";
		}
	}
	%this.dropTetrimino.clear(); // need?
	
	%this.pClear = %this.isCleared();

	// Todo: Cancel and do this immediately if there's an input
	%dropDelay = 400;
	%this.schedule(%dropDelay, "dropBoard", %rows);
	%this.schedule(%dropDelay+10, "nextTetrimino");
	
	%this.lines += %rowCount;
	$pref::KeppyMarbletris::linesCleared += %rowCount;
	//%this.level = mCeil(%this.lines / 10);

	// Todo: remove getword, fix level 10+
	//if(%this.level < 11)
	//  %this.fallDelay = 
	
	switch (%rowCount) {
		case 1:
			if(%this.tSpin)
				%score = 800;
			else if (%this.mSpin)
				%score = 200;
			else
				%score = 100;
			if(%this.pClear)
				%score += 800;
		case 2:
			if(%this.tSpin)
				%score = 1200;
			else if(%this.mSpin)
				%score = 400;
			else
				%score = 300;
			if(%this.pClear)
				%score += 1200;
		case 3:
			if(%this.tSpin)
				%score = 1600;
			else
				%score = 500;
			if(%this.pClear)
				%score += 2000;
		case 4:
			%score = 800;
			if(%this.pClear) {
				if(%this.b2b)
					%score += 3200;
				else
					%score += 2000;
			}
			%tt = true;
	}
	//todo: perfect clear
	
	%this.messageAdd(getWord(MissionList.clearNames, %rowCount-1));
	
	if(%this.tSpin)
		%this.messageAdd("T-Spin");
	else if(%this.mSpin)
		%this.messageAdd("Mini T-Spin");
	
	if(%this.pClear)
		%this.messageAdd("Perfect Clear");

	%b2bFlag = (%rowCount == 4 || %this.mSpin || %this.tSpin);
	%this.b2b = %this.b2b && %b2bFlag;
	
	if(%this.b2b) {
		%score *= 1.5;
		%this.messageAdd("Back to Back");
	}
		
	
	%this.b2b = %b2bFlag;
	
	%score *= %this.level;
	
	%this.streak++;
	
	%score += 50 * %this.streak * %this.level;
	
	%msg = %this.msg SPC "+" SPC "Level" SPC %this.level SPC "=" SPC %score;
	messageClient(LocalClientConnection, 'MsgItemPickup', %msg);
	
	%score += %this.rem;
	
	//
	
	debug("Score:" SPC %score);
	
	%gems = "";
	while(%score > 1000) {
		%gems = AddWord(%gems, 10);
		%score -= 1000;
	}
	%this.rem = %score % 100;
	%num = (%score-%this.rem) / 100;
	if(%num)
		%gems = AddWord(%gems, %num);
	
	//if(%tt) {
	//	%gems = AddWord(%gems, MissionList.timeTravelID);
	//}

	debug("Gems:" SPC %gems);
	%this.createGems(%gems);
	
	//LocalClientConnection.incBonusTime(%rowCount * 500);
	LocalClientConnection.incBonusTime(%dropDelay);
}

function KeppyMarbletris::getBoardMaxHeight(%this) {
	for(%y = 24; %y >= 0; %y--) { 
		for(%x = 0; %x < 10; %x++) {
			if(isObject(%this.board[%x, %y])) {
				return %y;
			}
		}
	}
	return 0;
}

function KeppyMarbletris::createGems(%this, %gems) {
	//%maxHeight = max(4, %this.getBoardMaxHeight()); // TODO sub cleared rows?
	%maxHeight = 20;
	// TODO could also just base max height on current level. I think always having a low height would make the game boring.
	%availableSlots = "";
	for(%y = 0; %y < 25; %y++) {
		for(%x = 0; %x < 10; %x++) {
			if(!%this.boardGems[%x, %y])
				%availableSlots = addField(%availableSlots, %x SPC %y);
		}
		if(%y >= %maxHeight && getFieldCount(%availableSlots) >= getWordCount(%gems))
			break;
	}
	//if(getFieldCount(%availableSlots) < getWordCount(%gems)) {
	//	error("Filled all gem slots??");
	//	return;
	//}
	for(%i = 0; %i < getWordCount(%gems); %i++) {
		%num = getWord(%gems, %i);
		%slotIndex = getRandom(0, getFieldCount(%availableSlots)-1);
		%slot = getField(%availableSlots, %slotIndex);
		%availableSlots = removeField(%availableSlots, %slotIndex);
		%this.schedule(500*%i, "createGem", %num, firstWord(%slot), restWords(%slot));
	}
}

// TODO the sounds should probably be in major key
function KeppyMarbletris::createGem(%this, %num, %x, %y) {
	%color = getWord(MissionList.gems, %num-1);
	%position = %x*2 SPC %y*2 SPC 0.25;
	
	//if(%num != MissionList.timeTravelID) {
		%dataBlock = "GemItem" @ %color;
		spawnEmitter(200, "GemEmitter" @ %color @ "Tetris", %position, false);
		spawnEmitter(200, LandMineSparkEmitter, %position, false);
		LocalClientConnection.play2D("score" @ %num @ "Sfx");
		%group = MarbletrisGems;
	//}
	//else {
	//	%position = VectorAdd(%position, "0 0 0.75");
	//	%dataBlock = TimeTravelItem;
	//	%group = MarbletrisObjects;
	//}
	%gem = new Item() {
		position = %position;
		dataBlock = %dataBlock;
		x = %x;
		y = %y;
		collideable = "0";
		static = "1";
		rotate = "1";
	};
	//MarbletrisObjects.add(%gem);
	//%this.currentGems++;
	%group.add(%gem);
	%gem._huntDatablock = "gemItem" @ %color;
	%this.boardGems[%x, %y] = %gem;
	if(%this.getBoardValue() > (%this.gemLevelRequirement - PlayGui.gemCount)) {
		if(!%this.messagedReady) {
			messageClient(LocalClientConnection, 'MsgItemPickup', "Level Up Ready");
			LocalClientConnection.playPitchedSound("gotalldiamonds"); // TODO only play one sound
		}
		%this.messagedReady = true;
	}
}

function KeppyMarbletris::getBoardValue(%this) {
	%count = 0;
	for(%i = 0; (%gem = MarbletrisGems.getObject(%i)) != -1; %i++) {
		%count += %gem._huntDatablock.huntExtraValue+1;
	}
	return %count;
}

function KeppyMarbletris::levelUp(%this) {
	%this.level += 1;
	%this.gemLevelRequirement += %this.level * 10;
	messageClient(LocalClientConnection, 'MsgItemPickup', "Level Up!"); // TODO sound
	LocalClientConnection.playPitchedSound("gotalldiamonds");
	%this.messagedReady = false;
}

function KeppyMarbletris::onGemCollected(%this, %gem) {
	%lastLevel = %this.level;
	//echo(PlayGui.gemCount);
	//echo(%this.gemLevelRequirement);
	%this.boardGems[%gem.x, %gem.y] = "";
	//%this.availableGemSlots = AddField(%this.availableGemSlots, %gem.position);
	debug("Gem count:" SPC PlayGui.gemCount SPC "Gem requirement:" SPC %this.gemLevelRequirement);
	if (PlayGui.gemCount >= %this.gemLevelRequirement) { // todo show requirement
		%this.levelUp();
	}
	%gem.delete();
	if(MarbletrisGems.getCount() == 0) {
		%this.destroyBoard();
	}
	//%this.gemsCollected++;
	//if(%this.gemsCollected == %this.currentGems) {
	//  %this.currentGems = %this.gemsCollected = 0;
	//  %this.destroyBoard();
	//}
}

function Tetrimino::hide(%this, %val) {
	for(%b = %this.start; %b; %b = %b.next) {
		%b.hide(%val);
	}
}

function KeppyMarbletris::hideBoard(%this, %val) {
  
}

package MarbletrisPackage {
	function OptionsGui::back(%this) {
		Parent::back(%this);
		MarbletrisControl.push();
	}
	function Gem::onPickup(%this,%obj,%user,%amount) { // TODO need this for TT too?
		Parent::onPickup(%this,%obj,%user,%amount);
		Marbletris.onGemCollected(%obj);
	}
	function escapeFromGame(%val) {
		input_escapeFromGame(%val);
		
		if(!LocalClientConnection.playing)
			return;
	}
	function useBlast(%val) {
		echo("blasting");

		Parent::useBlast(%val);
	}
	
	function performBlast() {
		echo("performing blast");
		Marbletris.blastBlocks();
		Parent::performBlast();
	}
	function test() {
		echo("package on");
	}
	
	function toggleEditor(%make) {
		Parent::toggleEditor(%make);
		if(RootGui.getContent() != EditorGui.getId())
			MarbletrisControl.push();
	}
		//$gamePaused = false;
		//moveCamera();
		
		//onNextFrame("pauseGame");
		//onNextFrame("onNextFrame", "setView");
		
		//schedule(50, 0, "eval", "pqubeControl.push();");
};

function clientCbOnServerLeave() {
	//GemItemPink.huntExtraValue = 0;
	//GemItemBlack.huntExtraValue = -2;
	//MarbletrisControl.pop();
	//TetrisControl.pop();
	deactivatePackage(MarbletrisPackage);
}