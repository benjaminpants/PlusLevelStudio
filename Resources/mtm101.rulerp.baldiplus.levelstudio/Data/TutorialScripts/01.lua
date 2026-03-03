-- nothing here is implemented/works yet, this is just so i know what i want a tutorial script to look lie

local step = 0
local waitingFor = nil


local steps = [
function()
	baldi:Talk("BAL_Ed_Tut_Welcome")
end,
function()
	editor:LockMovement(false)
	baldi:Talk("BAL_Ed_Tut_MovementTutorial")
end,
function()
	baldi:Talk("BAL_Ed_Tut_ResizeGrid")
	local startingSize = level.size
	waitingFor = function(delta)
		return (level.size ~= startingSize)
	end
end,

]


function Start()
	BaldiDoneTalking(nil)
end

function Update(delta)
	if (waitingFor ~= nil) then
		if (waitingFor(delta)) then
			waitingFor = nil
			BaldiDoneTalking(nil)
		end
	end
end

function BaldiDoneTalking(lastLine)
	step = step + 1
	steps[step]()
end