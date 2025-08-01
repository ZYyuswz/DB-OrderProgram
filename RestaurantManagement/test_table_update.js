// 简单的测试脚本，用于测试桌台状态更新API
async function testTableStatusUpdate() {
    try {
        console.log('开始测试桌台状态更新...');
        
        // 测试更新桌台2的状态为清洁中
        const response = await fetch('/api/tables/2/status', {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ status: '清洁中' })
        });
        
        console.log('响应状态:', response.status);
        console.log('响应状态文本:', response.statusText);
        
        if (response.ok) {
            const result = await response.json();
            console.log('更新成功:', result);
        } else {
            const error = await response.text();
            console.error('更新失败:', error);
        }
        
    } catch (error) {
        console.error('请求失败:', error);
    }
}

// 运行测试
testTableStatusUpdate();
