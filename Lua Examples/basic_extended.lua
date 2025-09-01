-- do NOT write any code outside of a function, as it will be executed when the script is initially loaded, and during that process, no global variables are defined yet.
-- however, defining variables outside a function is a-okay!
-- A basic example script, containing all extra functions as well

function SetupPlayerProperties()
	-- not technically necessary to include since these are all the defaults.
	return {
		walkSpeed=16,
		runSpeed=24,
		staminaDrop=10,
		staminaMax=100,
		staminaRise=20
	}
end

function Initialize()

end

function ExitedSpawn()
	self:SpawnNPCs()
	self:StartEventTimers()

end

function Update(delta)

end

function AllNotebooks()
	self:OpenExits(true)
end

function NotebookCollected(notebookPosition)
end


function OnItemUse(itemId, slot)
	return true
end

function AngerBaldi(value)
	return value
end

function AllNPCsSpawned()
end