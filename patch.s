	.text
REG_BASE		= 0x4000000
REG_DISPCNT		= 0x00
REG_DISPSTAT	= 0x04
REG_VCOUNT		= 0x06
REG_BG0CNT		= 0x08
REG_BG1CNT		= 0x0A
REG_BG2CNT		= 0x0C
REG_BG3CNT		= 0x0E
REG_BG0HOFS		= 0x10
REG_BG0VOFS		= 0x12
REG_BG1HOFS		= 0x14
REG_BG1VOFS		= 0x16
REG_BG2HOFS		= 0x18
REG_BG2VOFS		= 0x1A
REG_BG3HOFS		= 0x1C
REG_BG3VOFS		= 0x1E
REG_WIN0H		= 0x40
REG_WIN1H		= 0x42
REG_WIN0V		= 0x44
REG_WIN1V		= 0x46
REG_WININ		= 0x48
REG_WINOUT		= 0x4A
REG_BLDCNT		= 0x50
REG_BLDALPHA	= 0x52
REG_BLDY		= 0x54
REG_SG1CNT_L	= 0x60
REG_SG1CNT_H	= 0x62
REG_SG1CNT_X	= 0x64
REG_SG2CNT_L	= 0x68
REG_SG2CNT_H	= 0x6C
REG_SG3CNT_L	= 0x70
REG_SG3CNT_H	= 0x72
REG_SG3CNT_X	= 0x74
REG_SG4CNT_L	= 0x78
REG_SG4CNT_H	= 0x7c
REG_SGCNT_L		= 0x80
REG_SGCNT_H		= 0x82
REG_SOUNDCNT_X		= 0x84
REG_SGBIAS		= 0x88
REG_SGWR0_L		= 0x90
REG_FIFO_A_L	= 0xA0
REG_FIFO_A_H	= 0xA2
REG_FIFO_B_L	= 0xA4
REG_FIFO_B_H	= 0xA6
REG_DM0SAD		= 0xB0
REG_DM0DAD		= 0xB4
REG_DM0CNT_L	= 0xB8
REG_DM0CNT_H	= 0xBA
REG_DM1SAD		= 0xBC
REG_DM1DAD		= 0xC0
REG_DM1CNT_L	= 0xC4
REG_DM1CNT_H	= 0xC6
REG_DM2SAD		= 0xC8
REG_DM2DAD		= 0xCC
REG_DM2CNT_L	= 0xD0
REG_DM2CNT_H	= 0xD2
REG_DM3SAD		= 0xD4
REG_DM3DAD		= 0xD8
REG_DM3CNT_L	= 0xDC
REG_DM3CNT_H	= 0xDE
REG_TM0D		= 0x100
REG_TM0CNT		= 0x102
REG_IE			= 0x200
REG_IF			= 0x202
REG_P1			= 0x130
REG_P1CNT		= 0x132
REG_WAITCNT		= 0x204


install_handler:
	@r0 = address of interrupt handler
	mov r1,#0x04000000
	str r0,[r1,#-(0x04000000-0x03FFFFB0)]
	@install "my_irq"
	adr r0,my_irq
	str r0,[r1,#-(0x04000000-0x03FFFFB4)]

	ldr r0,=0xE5901200 @ldr r1,[r0,#REG_IE]
	str r0,[r1,#-(0x04000000-0x03FFFFA0)]
	ldr r0,=0xE3110801 @tst r1,#0x00010000
	str r0,[r1,#-(0x04000000-0x03FFFFA4)]
	ldr r0,=0x0510F050 @ldreq pc,[r0,#-0x50]
	str r0,[r1,#-(0x04000000-0x03FFFFA8)]
	ldr r0,=0xE510F04C @ldr pc,[r0,#-0x4C]
	str r0,[r1,#-(0x04000000-0x03FFFFAC)]

	@install tiny IRQ handler
	ldr r0,=0x03007FA0
	str r0,[r1,#-4]
	
	bx lr
my_irq:
	@r0 = reg_base
	ldr r2,[r0,#REG_P1]
	tst r2,#0x0308			@check for L+R+start
	ldrne pc,[r0,#-(0x04000000-0x03FFFFB0)] @to IRQ routine if not pressed
	stmfd sp!,{lr}  @let's save old LR, set a new one, and let the interrupt handler finish
	adr lr,my_irq1
	ldr pc,[r0,#-(0x04000000-0x03FFFFB0)] @jump to IRQ handler
my_irq1:
	@goes here after IRQ handler finishes

	@save old io values
	stmfd sp!,{r4-r7}
	mov r0,#REG_BASE
	ldr r4,[r0,#REG_IE]
	ldr r5,[r0,#REG_P1]
	ldrh r6,[r0,#REG_SOUNDCNT_X]
	ldrh r7,[r0,#REG_DISPCNT]
	
	@enable ints on Keypad, Game Pak
	@acknowoledge Vblank
	mov r3,#0x00013000
	str r3,[r0,#REG_IE]
	mov r3,#0xC0000000		@interrupt on start+sel
	orr r3,r3,#0x000C0000
	str r3,[r0,#REG_P1]
	strh r0,[r0,#REG_SOUNDCNT_X]	@sound off
	orr r3,r6,#0x80
	strh r3,[r0,#REG_DISPCNT]	@LCD off
	swi 0x030000
	mov r0,#REG_BASE
	mov r1,#0
	str r1,[r0,#REG_IE]
	@Loop to wait for letting go of Sel+start
loop:
	ldr r1,[r0,#REG_P1]
	and r1,r1,#0x000C
	cmp r1,#0x000C
	bne loop
	mvn r1,#0
	@acknowledge all interrupts
	str r1,[r0,#REG_IE]
	@restore interrupts
	str r4,[r0,#REG_IE]
	@restore joystick interrupt
	str r5,[r0,#REG_P1]
	@restore sound state
	strh r6,[r0,#REG_SOUNDCNT_X]
	@restore screen
	strh r7,[r0,#REG_DISPCNT]
	
	ldmfd sp!,{r4-r7}
	ldmfd sp!,{pc}
@	ldr pc,[r0,#-(0x04000000-0x03FFFFB0)] @to IRQ routine


@invocation:
@stmfd sp!,{r0,r1,r12,lr}
@mov r12,rA
@mov r1,rB
@mov r0,r12
@bl call_address
@ldmfd sp!,{r0,r1,r12,lr}
@ldr rB,=return_adress
@bx rB
