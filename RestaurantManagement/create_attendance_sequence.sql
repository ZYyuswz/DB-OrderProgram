-- 创建考勤序列
-- 首先检查序列是否存在
DECLARE
    seq_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO seq_count FROM USER_SEQUENCES WHERE SEQUENCE_NAME = 'ATTENDANCE_SEQ';
    
    IF seq_count = 0 THEN
        -- 获取当前ATTENDANCE表的最大ID
        DECLARE
            max_id NUMBER;
        BEGIN
            SELECT NVL(MAX(ATTENDANCEID), 0) INTO max_id FROM PUB.ATTENDANCE;
            
            -- 创建序列
            EXECUTE IMMEDIATE 'CREATE SEQUENCE PUB.ATTENDANCE_SEQ START WITH ' || (max_id + 1) || ' INCREMENT BY 1 NOCACHE NOCYCLE';
            
            DBMS_OUTPUT.PUT_LINE('ATTENDANCE_SEQ 序列创建成功，起始值：' || (max_id + 1));
        END;
    ELSE
        DBMS_OUTPUT.PUT_LINE('ATTENDANCE_SEQ 序列已存在');
    END IF;
END;
/

-- 验证序列
SELECT SEQUENCE_NAME, MIN_VALUE, MAX_VALUE, INCREMENT_BY, LAST_NUMBER 
FROM USER_SEQUENCES 
WHERE SEQUENCE_NAME = 'ATTENDANCE_SEQ';
